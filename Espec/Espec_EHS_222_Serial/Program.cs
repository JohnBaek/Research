using System.Text;

namespace Espec_EHS_222_Serial;

internal static class Program
{
    private const string Host = "192.168.100.50";
    private const int Port = 5000;
    private const int Address = 1;
    private const string Delimiter = "\r\n";

    private static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("==================================================");
        Console.WriteLine(" ESPEC HAST EHS 시리즈 온·습도 진단 콘솔 (Ethernet)");
        Console.WriteLine("==================================================");
        Console.WriteLine("기준: HAST 통신 매뉴얼 4013304025610 / Standard(Ebs OFF)");
        Console.WriteLine("확인한 챔버값: 19200 / 8N1 / 주소1 / CRLF");
        Console.WriteLine("보레이트·8N1은 컨버터에서 설정합니다. TCP 연결 성공은 챔버 응답 확인과 별개입니다.");

        var transport = new TcpEspecTransport(new TcpConnectionSettings
        {
            Host = Host, Port = Port, Delimiter = Delimiter
        }) { Trace = Dump };
        using var client = new EspecClient(transport, Address);
        try { client.Open(); }
        catch (Exception ex) { Console.WriteLine($"[연결 실패] {ex.Message}"); return; }
        Console.WriteLine($"[TCP 연결됨] {client.Describe()}");
        Print(client.Query("ROM?"));
        Print(client.Query("MON?"));
        Help();

        while (true)
        {
            Console.Write($"[addr={client.Address}, delim={Escape(client.Delimiter)}] 명령> ");
            string? input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input) || input.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;
            try
            {
                string[] tokens = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                string op = tokens[0].ToLowerInvariant();
                string arg = tokens.Length > 1 ? tokens[1] : "";
                switch (op)
                {
                    case ":help": Help(); break;
                    case ":addr":
                        if (!int.TryParse(arg, out int addr)) throw new ArgumentException("사용법: :addr 1");
                        client.Address = addr;
                        break;
                    case ":delim":
                        client.Delimiter = arg.ToLowerInvariant() switch
                        {
                            "cr" => "\r", "lf" => "\n", "crlf" => "\r\n",
                            _ => throw new ArgumentException("사용법: :delim cr|lf|crlf")
                        };
                        break;
                    case ":sniff":
                    case "sniff": // Accept the common missing-colon typo.
                        if (arg.Length == 0) throw new ArgumentException("사용법: :sniff ROM?");
                        Print(client.Query(arg, 1500));
                        break;
                    case ":trig":
                        if (arg.Length == 0) throw new ArgumentException("사용법: :trig ROM?");
                        Console.WriteLine("E-BUS + Echo OFF 전용 진단입니다. 현재 Ebs OFF에는 일반 ROM?/MON? 사용.");
                        Print(client.QueryWithTrigger(arg));
                        break;
                    case ":diag": Diagnostic(client); break;
                    case ":scan": Scan(client); break;
                    case ":reconnect":
                        client.Open();
                        Console.WriteLine("[TCP 재연결됨]");
                        break;
                    default:
                        if (input.StartsWith(':')) throw new ArgumentException("알 수 없는 명령. :help 참고");
                        Print(client.Query(input));
                        break;
                }
            }
            catch (Exception ex) { Console.WriteLine($"[오류] {ex.Message}"); }
        }
    }

    private static void Diagnostic(EspecClient client)
    {
        string saved = client.Delimiter;
        Console.WriteLine("종결문자별 수신 비교. 바이트 수신만으로 챔버 정상/노이즈 원인을 확정하지 않습니다.");
        try
        {
            foreach (string delim in new[] { "\r\n", "\r", "\n" })
            {
                client.Delimiter = delim;
                Console.WriteLine($"종결문자={Escape(delim)}");
                Print(client.Query("ROM?", 1500));
            }
        }
        finally { client.Delimiter = saved; }
    }

    private static void Scan(EspecClient client)
    {
        int saved = client.Address;
        int valid = 0;
        Console.WriteLine("주소1~16 ROM? 순차 조회. 현재 구분자만 사용하며 정상 모델/버전 형식만 후보로 인정합니다.");
        try
        {
            for (int addr = 1; addr <= 16; addr++)
            {
                client.Address = addr;
                Console.WriteLine($"--- 주소 {addr} ---");
                var result = client.Query("ROM?", 1500);
                Print(result);
                if (result.Ok) valid++;
            }
        }
        finally { client.Address = saved; }
        Console.WriteLine($"스캔 완료: 정상 ROM 형식 후보 {valid}개. 지연 응답 가능성이 있어 개별 주소로 반복 확인하세요.");
    }

    private static void Print(EspecResult result)
    {
        Console.WriteLine($"[{result.Kind}] {result.Command}: {result.Detail}");
        if (result.Raw.Length > 0) Console.WriteLine($"  DATA: {Escape(result.Raw)}");
        if (result.Ok && result.Command == "MON?" && EspecClient.TryMonitor(result.Raw, out var p))
            Console.WriteLine($"  온도 {p[0]}℃ / 습도 {p[1]}%RH / 모드 {p[2]} / 알람 {p[3]}건");
    }

    private static void Dump(string direction, byte[] bytes)
    {
        Console.WriteLine($"  {direction}: {bytes.Length} bytes");
        if (bytes.Length == 0) return;
        Console.WriteLine($"  HEX: {string.Join(" ", bytes.Select(b => b.ToString("X2")))}");
        string ascii = new(bytes.Select(b => b is >= 32 and <= 126 ? (char)b : '.').ToArray());
        Console.WriteLine($"  ASC: {ascii}");
    }
    private static string Escape(string value) => value.Replace("\r", "\\r").Replace("\n", "\\n");

    private static void Help()
    {
        Console.WriteLine("ROM? / MON? : HAST 조회 (운전·설정 변경 명령은 받지 않음)");
        Console.WriteLine(":sniff ROM? : 송수신 HEX와 판정 / :scan : 주소 스캔 / :diag : 종결문자 비교");
        Console.WriteLine(":addr 1 / :delim crlf / :reconnect / :help / exit");
        Console.WriteLine(":trig ROM? : E-BUS Echo OFF용 선택 진단 (현재 Standard에는 불필요)");
        Console.WriteLine("Valid=정상 형식, NoData=무수신, EchoOnly=에코만, InvalidBytes=깨진 수신,");
        Console.WriteLine("Incomplete=종결 미완료, DeviceError=NA 오류, Unverified=미확인 형식, TransportError=TCP 오류");
    }
}
