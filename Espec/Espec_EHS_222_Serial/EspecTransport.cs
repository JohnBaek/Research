using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace Espec_EHS_222_Serial;

public sealed record WireCapture(byte[] Sent, byte[] Received, byte[] Pending, string? Error);

public interface IEspecTransport : IDisposable
{
    string Delimiter { get; set; }
    void Open();
    string Describe();
    WireCapture Exchange(string frame, int windowMs, bool drain = true);
}

/// <summary>TCP transparent converter. UART settings belong to the converter.</summary>
public sealed class TcpEspecTransport(TcpConnectionSettings settings) : IEspecTransport
{
    private TcpClient? _client;
    private NetworkStream? _stream;
    public string Delimiter { get; set; } = settings.Delimiter;
    public Action<string, byte[]>? Trace { get; set; }
    private const int Limit = 16384;

    public void Open()
    {
        Dispose();
        var client = new TcpClient { NoDelay = true };
        try
        {
            using var timeout = new CancellationTokenSource(settings.ConnectTimeoutMs);
            client.ConnectAsync(settings.Host, settings.Port, timeout.Token).AsTask().GetAwaiter().GetResult();
            _client = client;
            _stream = client.GetStream();
            _stream.WriteTimeout = settings.WriteTimeoutMs;
        }
        catch { client.Dispose(); throw; }
    }

    public WireCapture Exchange(string frame, int windowMs, bool drain = true)
    {
        if (windowMs <= 0) throw new ArgumentOutOfRangeException(nameof(windowMs));
        if (string.IsNullOrEmpty(Delimiter)) throw new InvalidOperationException("종결문자가 비어 있습니다.");
        byte[] sent = Encoding.ASCII.GetBytes(frame + Delimiter);
        var received = new List<byte>();
        var pending = new List<byte>();
        string? error = null;
        try
        {
            var stream = _stream ?? throw new InvalidOperationException("연결되지 않았습니다. :reconnect 사용");
            byte[] buffer = new byte[512];
            if (drain)
            {
                while (stream.DataAvailable && pending.Count < Limit)
                {
                    int n = stream.Read(buffer, 0, Math.Min(buffer.Length, Limit - pending.Count));
                    if (n == 0) throw new IOException("TCP 연결 종료");
                    pending.AddRange(buffer.AsSpan(0, n).ToArray());
                }
                if (pending.Count > 0) Trace?.Invoke("이전/비동기 RX", pending.ToArray());
                if (pending.Count == Limit) throw new IOException("이전 수신 데이터 한도 초과");
            }

            Trace?.Invoke("TX", sent);
            stream.Write(sent);
            var clock = Stopwatch.StartNew();
            while (clock.ElapsedMilliseconds < windowMs && received.Count < Limit)
            {
                stream.ReadTimeout = Math.Max(1, Math.Min(100, windowMs - (int)clock.ElapsedMilliseconds));
                try
                {
                    int n = stream.Read(buffer, 0, Math.Min(buffer.Length, Limit - received.Count));
                    if (n == 0) { error = "TCP 상대가 연결을 종료했습니다. :reconnect 사용"; break; }
                    received.AddRange(buffer.AsSpan(0, n).ToArray());
                }
                catch (IOException ex) when (ex.InnerException is SocketException se &&
                    se.SocketErrorCode is SocketError.TimedOut or SocketError.WouldBlock) { }
            }
            if (received.Count == Limit) error = "수신 데이터 한도 초과";
        }
        catch (Exception ex) when (ex is IOException or SocketException or InvalidOperationException or ObjectDisposedException)
        {
            error = ex.Message;
        }
        Trace?.Invoke("RX", received.ToArray());
        return new WireCapture(sent, received.ToArray(), pending.ToArray(), error);
    }

    public string Describe() => $"TCP {settings.Host}:{settings.Port}";
    public void Dispose()
    {
        _stream?.Dispose();
        _client?.Dispose();
        _stream = null;
        _client = null;
    }
}

public sealed record TcpConnectionSettings
{
    public required string Host { get; init; }
    public required int Port { get; init; }
    public string Delimiter { get; init; } = "\r\n";
    public int ConnectTimeoutMs { get; init; } = 3000;
    public int ReadTimeoutMs { get; init; } = 2000;
    public int WriteTimeoutMs { get; init; } = 2000;
}
