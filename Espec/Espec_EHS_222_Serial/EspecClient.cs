using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Espec_EHS_222_Serial;

public enum ReplyKind { Valid, NoData, EchoOnly, InvalidBytes, Incomplete, DeviceError, Unverified, TransportError }
public sealed record EspecResult(string Command, string Raw, ReplyKind Kind, string Detail, WireCapture Capture)
{
    public bool Ok => Kind == ReplyKind.Valid;
}

/// <summary>HAST EHS standard mode. Manual 4013304025610.</summary>
public sealed class EspecClient(IEspecTransport transport, int address) : IDisposable
{
    private int _address = CheckAddress(address);
    public int Address { get => _address; set => _address = CheckAddress(value); }
    public string Delimiter
    {
        get => transport.Delimiter;
        set
        {
            if (value is not ("\r" or "\n" or "\r\n")) throw new ArgumentException("CR/LF/CRLF만 허용");
            transport.Delimiter = value;
        }
    }
    public int MonitorDelayMs { get; init; } = 250;
    public void Open() => transport.Open();
    public string Describe() => transport.Describe();

    public EspecResult Query(string command, int windowMs = 2000)
    {
        command = Normalize(command);
        var capture = transport.Exchange($"{Address:00},{command}", windowMs);
        Thread.Sleep(MonitorDelayMs);
        return Analyze(command, capture);
    }

    // Explicit diagnostic only: E-BUS + Echo OFF. Preserve both capture windows.
    public EspecResult QueryWithTrigger(string command, int windowMs = 2000)
    {
        command = Normalize(command);
        var initial = transport.Exchange($"{Address:00},{command}", 500);
        if (initial.Error is not null) return Analyze(command, initial);
        Thread.Sleep(MonitorDelayMs);
        var triggered = transport.Exchange($"{Address:00},G", windowMs, drain: false);
        Thread.Sleep(MonitorDelayMs);
        return Analyze(command, new WireCapture(
            initial.Sent.Concat(triggered.Sent).ToArray(),
            initial.Received.Concat(triggered.Received).ToArray(),
            initial.Pending, triggered.Error));
    }

    private EspecResult Analyze(string command, WireCapture capture)
    {
        var bytes = capture.Received;
        EspecResult Result(ReplyKind kind, string detail, string raw = "") =>
            new(command, raw, kind, detail, capture);
        if (capture.Error is not null) return Result(ReplyKind.TransportError, capture.Error);
        if (bytes.Length == 0) return Result(ReplyKind.NoData, "수신 창 안에 0 bytes. 원인은 아직 미확정");
        if (bytes.Any(b => b != 9 && b != 10 && b != 13 && (b < 32 || b > 126)))
            return Result(ReplyKind.InvalidBytes, "비 ASCII/제어 바이트 수신. 정상 HAST 응답으로 판정 불가");

        string text = Encoding.ASCII.GetString(bytes);
        string[] parts = text.Split(Delimiter, StringSplitOptions.None);
        var complete = parts.Take(parts.Length - 1).Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();
        bool Echo(string line) =>
            line.Equals($"{Address:00},{command}", StringComparison.OrdinalIgnoreCase) ||
            line.Equals($"{Address},{command}", StringComparison.OrdinalIgnoreCase) ||
            line.Equals($"{Address:00},G", StringComparison.OrdinalIgnoreCase) ||
            line.Equals($"{Address},G", StringComparison.OrdinalIgnoreCase);
        var replies = complete.Where(s => !Echo(s)).ToArray();
        if (parts[^1].Length != 0)
            return Result(ReplyKind.Incomplete, "수신은 있었으나 설정 종결문자로 끝나지 않음", text);
        if (replies.Length == 0)
            return Result(ReplyKind.EchoOnly, "완성된 장비 데이터 응답 없이 에코/빈 프레임만 수신", text);

        foreach (string reply in replies)
        {
            if (Regex.IsMatch(reply, @"^NA\s*:", RegexOptions.IgnoreCase))
                return Result(ReplyKind.DeviceError, "장비 오류 응답", reply);
        }
        if (replies.Length != 1)
            return Result(ReplyKind.Unverified, "데이터 응답이 여러 줄임. 지연 응답/프레임 형식 확인 필요", text);
        string raw = replies[0];
        if (command.Equals("ROM?", StringComparison.OrdinalIgnoreCase) &&
            Regex.IsMatch(raw, @"^EHS[-A-Z0-9()]*\s+Ver\.?\s*\d+(?:\.\d+)+$", RegexOptions.IgnoreCase))
            return Result(ReplyKind.Valid, "HAST 모델/버전 응답 형식 확인", raw);
        if (command.Equals("MON?", StringComparison.OrdinalIgnoreCase) && TryMonitor(raw, out _))
            return Result(ReplyKind.Valid, "HAST 온습도/모드/알람 응답 형식 확인", raw);
        return Result(ReplyKind.Unverified, "ASCII 프레임 수신. 명령별 정상 형식은 확인되지 않음", raw);
    }

    public static bool TryMonitor(string raw, out string[] fields)
    {
        fields = raw.Split(',', StringSplitOptions.TrimEntries); // Keep empty fields.
        return fields.Length == 4 &&
            double.TryParse(fields[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double t) && double.IsFinite(t) &&
            double.TryParse(fields[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double h) && double.IsFinite(h) && h is >= 0 and <= 100 &&
            fields[2] is "OFF" or "CONSTANT" or "RUN" &&
            int.TryParse(fields[3], NumberStyles.None, CultureInfo.InvariantCulture, out int count) && count >= 0;
    }

    private static string Normalize(string command)
    {
        command = command.Trim().ToUpperInvariant();
        if (command.Length == 0 || command.Any(c => c < 32 || c > 126) || !command.Contains('?'))
            throw new ArgumentException("조회 명령만 입력하세요. 예: ROM? / MON?");
        return command;
    }
    private static int CheckAddress(int value) => value is >= 1 and <= 16
        ? value : throw new ArgumentOutOfRangeException(nameof(value), "주소는 1~16");
    public void Dispose() => transport.Dispose();
}
