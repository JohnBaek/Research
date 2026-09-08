using System.IO.Ports;
using System.Net.Sockets;
using System.Text;

namespace Espec_EHS_222_Serial;

/// <summary>
/// 물리 전송 계층 추상화. 프레임(주소,명령)을 보내고 구분자까지의 응답 한 줄을 받는다.
/// 시리얼(RS-485 직결)이든 TCP(RS-485↔Ethernet 컨버터)든 이 계약만 지키면 된다.
/// </summary>
public interface IEspecTransport : IDisposable
{
    void Open();

    /// <summary>frame(구분자 미포함)에 구분자를 붙여 전송하고, 구분자까지의 응답을 Trim해서 반환.</summary>
    string SendCommand(string frame);

    string Describe();
}

/// <summary>RS-485 시리얼 포트 직결.</summary>
public sealed class SerialEspecTransport : IEspecTransport
{
    private readonly SerialPort _port;
    private readonly SerialConnectionSettings _s;

    public SerialEspecTransport(SerialConnectionSettings s)
    {
        _s = s;
        _port = new SerialPort(s.PortName, s.BaudRate, s.Parity, s.DataBits, s.StopBits)
        {
            Handshake = Handshake.None,       // Xon/Xoff, Si/So 없음(사양서 고정)
            ReadTimeout = s.ReadTimeoutMs,
            WriteTimeout = s.WriteTimeoutMs,
            NewLine = s.Delimiter,
            Encoding = Encoding.ASCII,
        };
    }

    public void Open()
    {
        if (!_port.IsOpen) _port.Open();
        _port.DiscardInBuffer();
        _port.DiscardOutBuffer();
    }

    public string SendCommand(string frame)
    {
        _port.DiscardInBuffer();
        _port.WriteLine(frame);           // NewLine(구분자) 자동 부착
        return _port.ReadLine().Trim();   // 타임아웃 시 TimeoutException
    }

    public string Describe() =>
        $"Serial {_s.PortName} @ {_s.BaudRate}bps {_s.DataBits}"
        + $"{ParityShort(_s.Parity)}{(_s.StopBits == StopBits.Two ? "2" : "1")}, "
        + $"구분자={Escape(_s.Delimiter)}";

    public void Dispose()
    {
        if (_port.IsOpen) _port.Close();
        _port.Dispose();
    }

    private static string ParityShort(Parity p) =>
        p switch { Parity.Even => "E", Parity.Odd => "O", _ => "N" };

    private static string Escape(string s) => s.Replace("\r", "\\r").Replace("\n", "\\n");
}

/// <summary>
/// RS-485↔Ethernet 컨버터(TCP 서버 모드)에 TCP 클라이언트로 접속.
/// 컨버터는 소켓으로 받은 바이트를 그대로 RS-485로 흘려보내므로 페이로드는 시리얼과 동일하다.
/// </summary>
public sealed class TcpEspecTransport : IEspecTransport
{
    private readonly TcpConnectionSettings _s;
    private readonly byte[] _delimBytes;
    private TcpClient? _client;
    private NetworkStream? _stream;

    public TcpEspecTransport(TcpConnectionSettings s)
    {
        _s = s;
        _delimBytes = Encoding.ASCII.GetBytes(s.Delimiter);
    }

    public void Open()
    {
        _client = new TcpClient { NoDelay = true };
        if (!_client.ConnectAsync(_s.Host, _s.Port).Wait(_s.ConnectTimeoutMs))
        {
            _client.Dispose();
            _client = null;
            throw new TimeoutException($"{_s.Host}:{_s.Port} 접속 타임아웃");
        }
        _stream = _client.GetStream();
        _stream.ReadTimeout = _s.ReadTimeoutMs;
        _stream.WriteTimeout = _s.WriteTimeoutMs;
        DrainInput();
    }

    public string SendCommand(string frame)
    {
        if (_stream is null) throw new InvalidOperationException("연결되지 않았습니다.");

        DrainInput();
        byte[] payload = Encoding.ASCII.GetBytes(frame + _s.Delimiter);
        _stream.Write(payload, 0, payload.Length);
        return ReadUntilDelimiter();
    }

    /// <summary>구분자가 나타날 때까지 1바이트씩 누적해서 읽는다.</summary>
    private string ReadUntilDelimiter()
    {
        var buffer = new List<byte>(64);
        var one = new byte[1];
        try
        {
            while (true)
            {
                int n = _stream!.Read(one, 0, 1);
                if (n == 0) // 상대가 연결 종료
                    throw new IOException("연결이 닫혔습니다(응답 미완료).");
                buffer.Add(one[0]);
                if (EndsWith(buffer, _delimBytes))
                {
                    buffer.RemoveRange(buffer.Count - _delimBytes.Length, _delimBytes.Length);
                    break;
                }
            }
        }
        catch (IOException ex) when (ex.InnerException is SocketException se
            && se.SocketErrorCode == SocketError.TimedOut)
        {
            // 클라이언트 상위 계층에서 통일되게 처리하도록 TimeoutException으로 변환.
            throw new TimeoutException("응답 타임아웃(무응답)");
        }

        return Encoding.ASCII.GetString(buffer.ToArray()).Trim();
    }

    private void DrainInput()
    {
        if (_stream is null) return;
        while (_stream.DataAvailable)
        {
            var junk = new byte[256];
            _ = _stream.Read(junk, 0, junk.Length);
        }
    }

    private static bool EndsWith(List<byte> buf, byte[] suffix)
    {
        if (buf.Count < suffix.Length) return false;
        for (int i = 0; i < suffix.Length; i++)
            if (buf[buf.Count - suffix.Length + i] != suffix[i]) return false;
        return true;
    }

    public string Describe() =>
        $"TCP {_s.Host}:{_s.Port}, 구분자={_s.Delimiter.Replace("\r", "\\r").Replace("\n", "\\n")}";

    public void Dispose()
    {
        _stream?.Dispose();
        _client?.Dispose();
    }
}
