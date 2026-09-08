using System.IO.Ports;

namespace Espec_EHS_222_Serial;

/// <summary>
/// ESPEC Platinous J 시리즈 프로토콜 계층(전송 방식과 무관).
/// 명령 형식 : "주소,메인명령[,옵션파라미터]" + 구분자
/// 응답 형식 : "응답데이터"  (실패 시 "NA:에러메시지")  — STD 프로토콜, 에코백 없음.
/// </summary>
public sealed class EspecClient : IDisposable
{
    private readonly IEspecTransport _transport;
    private readonly int _address;

    /// <summary>같은 주소로 연속 명령 전송 시 최소 대기(모니터 명령 0.2초 이상 권장).</summary>
    public int MonitorDelayMs { get; init; } = 250;

    public EspecClient(IEspecTransport transport, int address)
    {
        _transport = transport;
        _address = address;
    }

    public void Open() => _transport.Open();

    public string Describe() => _transport.Describe();

    /// <summary>명령 전송 후 결과를 구조화해서 반환. 통신 오류는 예외 대신 결과에 담는다.</summary>
    public EspecResult Query(string mainCommand)
    {
        try
        {
            string frame = $"{_address:00},{mainCommand}"; // 주소는 01 처럼 2자리 표기
            string raw = _transport.SendCommand(frame);
            Thread.Sleep(MonitorDelayMs);                  // 사양서 권고 지연

            bool isError = raw.StartsWith("NA:", StringComparison.OrdinalIgnoreCase);
            return new EspecResult(mainCommand, raw, !isError,
                isError ? raw[3..].Trim() : null);
        }
        catch (TimeoutException)
        {
            return new EspecResult(mainCommand, "", false, "응답 타임아웃(무응답)");
        }
        catch (Exception ex)
        {
            return new EspecResult(mainCommand, "", false, ex.Message);
        }
    }

    public void Dispose() => _transport.Dispose();
}

/// <summary>시리얼(RS-485 직결) 설정. 챔버 패널 통신설정과 일치시켜야 한다.</summary>
public sealed record SerialConnectionSettings
{
    public required string PortName { get; init; }
    public int BaudRate { get; init; } = 9600;              // 4800 / 9600 / 19200
    public Parity Parity { get; init; } = Parity.None;      // None / Even / Odd
    public int DataBits { get; init; } = 8;                 // 7 / 8
    public StopBits StopBits { get; init; } = StopBits.One; // One / Two
    public string Delimiter { get; init; } = "\r\n";        // 챔버 설정과 동일(CR / CRLF 등)
    public int ReadTimeoutMs { get; init; } = 2000;
    public int WriteTimeoutMs { get; init; } = 2000;
}

/// <summary>TCP(RS-485↔Ethernet 컨버터) 설정.</summary>
public sealed record TcpConnectionSettings
{
    public required string Host { get; init; }              // 컨버터 IP
    public required int Port { get; init; }                 // 컨버터 TCP 포트
    public string Delimiter { get; init; } = "\r\n";        // 챔버 설정과 동일해야 함
    public int ConnectTimeoutMs { get; init; } = 3000;
    public int ReadTimeoutMs { get; init; } = 2000;
    public int WriteTimeoutMs { get; init; } = 2000;
}

/// <summary>명령 1건의 실행 결과.</summary>
public sealed record EspecResult(string Command, string Raw, bool Ok, string? Error);
