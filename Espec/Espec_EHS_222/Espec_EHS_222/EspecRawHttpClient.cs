using System.Net.Sockets;
using System.Text;

namespace Espec_EHS_222;


/// <summary>
/// ESPEC 웹매니저 (lighttpd + Ruby FCGI) 전용 raw HTTP 클라이언트.
/// 서버가 malformed HTTP 를 응답하기 때문에 표준 HttpClient 는 사용 불가.
/// </summary>
public class EspecRawHttpClient
{
    private readonly string _host;
    private readonly int _port;
    private string? _sessionCookie;
    
    public EspecRawHttpClient(string host, int port = 80)
    {
        _host = host;
        _port = port;
    }
    
    public string? SessionCookie => _sessionCookie;
    
    /// <summary>
    /// GET 요청. body 부분만 반환 (JSON 또는 HTML).
    /// Set-Cookie 가 있으면 자동으로 세션 유지.
    /// </summary>
    public async Task<EspecResponse> GetAsync(string path, CancellationToken ct = default)
    {
        using TcpClient tcp = new TcpClient();
        tcp.ReceiveTimeout = 5000;
        tcp.SendTimeout = 5000;
        await tcp.ConnectAsync(_host, _port, ct);
        using NetworkStream stream = tcp.GetStream();

        // 요청 조립
        StringBuilder req = new();
        req.Append($"GET {path} HTTP/1.1\r\n");
        req.Append($"Host: {_host}\r\n");
        req.Append("User-Agent: Mozilla/5.0 AppleWebKit/537.36\r\n");
        req.Append("Accept: text/javascript, text/html, application/xml, text/xml, */*\r\n");
        req.Append("X-Requested-With: XMLHttpRequest\r\n");
        req.Append("X-Prototype-Version: 1.6.0.2\r\n");
        req.Append($"Referer: http://{_host}/main/login.html\r\n");
        if (_sessionCookie != null)
            req.Append($"Cookie: {_sessionCookie}\r\n");
        req.Append("Connection: close\r\n\r\n");

        await stream.WriteAsync(Encoding.ASCII.GetBytes(req.ToString()), ct);

        // 전체 응답 읽기 (Connection: close 로 EOF 까지)
        using MemoryStream ms = new();
        byte[] buffer = new byte[8192];
        int read;
        while ((read = await stream.ReadAsync(buffer, ct)) > 0)
            ms.Write(buffer, 0, read);

        return ParseResponse(ms.ToArray());
    }

    /// <summary>
    /// malformed 응답을 관대하게 파싱.
    /// - HTTP status line 추출
    /// - Set-Cookie 자동 처리
    /// - Ruby FCGI 디버그 dump 무시하고 body 추출
    /// </summary>
    private EspecResponse ParseResponse(byte[] raw)
    {
        string text = Encoding.UTF8.GetString(raw);
        
        // 마지막 "\r\n\r\n" 이 헤더/body 구분자 (중간에 FCGI dump 있어도 안전)
        int sepIdx = text.LastIndexOf("\r\n\r\n");
        string headerBlock = sepIdx >= 0 ? text[..sepIdx] : text;
        string body = sepIdx >= 0 ? text[(sepIdx + 4)..] : "";

        // Status code 추출
        int statusCode = 0;
        string[] lines = headerBlock.Split("\r\n");
        if (lines.Length > 0 && lines[0].StartsWith("HTTP/"))
        {
            string[] parts = lines[0].Split(' ', 3);
            if (parts.Length >= 2) int.TryParse(parts[1], out statusCode);
        }

        // Set-Cookie 추출 및 세션 유지
        foreach (string line in lines)
        {
            if (line.StartsWith("Set-Cookie:", StringComparison.OrdinalIgnoreCase))
            {
                string val = line["Set-Cookie:".Length..].Trim();
                int semi = val.IndexOf(';');
                _sessionCookie = semi > 0 ? val[..semi] : val;
            }
        }

        return new EspecResponse(statusCode, body);
    }
}

public record EspecResponse(int StatusCode, string Body)
{
    public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;
}