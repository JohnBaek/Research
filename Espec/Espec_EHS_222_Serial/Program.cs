namespace Espec_EHS_222_Serial;

/// <summary>
/// ESPEC Platinous J 시리즈(EHS-222 등) 온·습도 읽기 테스트 콘솔.
/// RS-485 ↔ Ethernet 컨버터(TCP 서버, 투명 전송) 접속 전용.
///
/// 흐름 : 연결 → 장비정보(ROM?, TYPE?) → 온습도(MON?, TEMP?, HUMI?) → 대화형 명령.
/// </summary>
internal static class Program
{
    // ── 접속 설정(현장에 맞게 여기만 수정) ──────────────────────
    private const string Host = "192.168.100.50";  // 컨버터 IP
    private const int Port = 5000;                  // 컨버터 TCP 포트
    private const int Address = 1;                  // 챔버 주소(1~16)
    private const string Delimiter = "\r\n";        // 챔버 패널 설정과 동일(CR / CRLF)

    private static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        PrintHeader();

        var transport = new TcpEspecTransport(new TcpConnectionSettings
        {
            Host = Host,
            Port = Port,
            Delimiter = Delimiter,
        });

        using var client = new EspecClient(transport, Address);

        try
        {
            client.Open();
            Console.WriteLine($"\n[연결됨] {client.Describe()}, 주소={Address}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[연결 실패] {ex.Message}");
            Console.WriteLine("→ 컨버터 IP/포트, 네트워크, 챔버-컨버터 시리얼 설정을 확인하세요.");
            return;
        }

        RunDeviceInfo(client);
        RunTempHumi(client);
        RunInteractive(client);

        Console.WriteLine("\n종료합니다.");
    }

    // ── 1) 장비 정보 ─────────────────────────────────────────────
    private static void RunDeviceInfo(EspecClient client)
    {
        Console.WriteLine("=== 장비 정보 ===");

        var rom = client.Query("ROM?");
        Print(rom, "ROM 버전");

        if (!rom.Ok && string.IsNullOrEmpty(rom.Raw))
        {
            Console.WriteLine("→ 첫 명령이 무응답입니다. 구분자/주소, 컨버터의 시리얼 파라미터(보레이트/패리티 등)를 재확인하세요.\n");
            return;
        }

        var type = client.Query("TYPE?");
        Print(type, "장비 타입");
        if (type.Ok)
        {
            // 응답 예: "T, T, P-300, 105.0" → 습식(2번째) 있으면 온습도 챔버.
            var p = SplitCsv(type.Raw);
            bool hasHumidity = p.Length >= 4; // 건구,습구,제어기,상한 = 온습도 / 건구,제어기,상한 = 온도전용
            Console.WriteLine($"   → 판정: {(hasHumidity ? "온도+습도 챔버" : "온도 전용 챔버")}");
        }
        Console.WriteLine();
    }

    // ── 2) 온·습도 읽기 ─────────────────────────────────────────
    private static void RunTempHumi(EspecClient client)
    {
        Console.WriteLine("=== 온·습도 측정값 ===");

        // MON? : 측정온도, [측정습도], 운전모드, 알람수  (예: "23.0, 85, CONSTANT, 0")
        var mon = client.Query("MON?");
        Print(mon, "MON?(운전상태)");
        if (mon.Ok)
        {
            var p = SplitCsv(mon.Raw);
            if (p.Length >= 4) // 온습도 챔버
                Console.WriteLine($"   → 온도 {p[0]}℃ / 습도 {p[1]}%RH / 모드 {p[2]} / 알람 {p[3]}건");
            else if (p.Length == 3) // 온도전용(습도 필드 생략)
                Console.WriteLine($"   → 온도 {p[0]}℃ / 모드 {p[1]} / 알람 {p[2]}건 (온도전용)");
        }

        // TEMP? : 측정온도, 설정온도, 상한알람, 하한알람
        var temp = client.Query("TEMP?");
        Print(temp, "TEMP?(온도)");
        if (temp.Ok)
        {
            var p = SplitCsv(temp.Raw);
            if (p.Length >= 2)
                Console.WriteLine($"   → 현재 {p[0]}℃ / 설정 {p[1]}℃");
        }

        // HUMI? : 측정습도, 설정습도, 상한, 하한  (온도전용이면 NA:INVALID REQ)
        var humi = client.Query("HUMI?");
        Print(humi, "HUMI?(습도)");
        if (humi.Ok)
        {
            var p = SplitCsv(humi.Raw);
            if (p.Length >= 2)
                Console.WriteLine($"   → 현재 {p[0]}%RH / 설정 {p[1]}%RH");
        }
        else if (humi.Error?.Contains("INVALID", StringComparison.OrdinalIgnoreCase) == true)
        {
            Console.WriteLine("   → 이 챔버는 습도 미지원(온도 전용)으로 보입니다.");
        }
        Console.WriteLine();
    }

    // ── 3) 대화형 명령 ─────────────────────────────────────────
    private static void RunInteractive(EspecClient client)
    {
        Console.WriteLine("=== 대화형 모드 ===");
        Console.WriteLine("명령을 입력하면 그대로 전송합니다(주소는 자동 부착). 예: MON?  /  TEMP?  /  TYPE?");
        Console.WriteLine("종료하려면 빈 줄 입력 또는 'exit'.\n");

        while (true)
        {
            Console.Write("명령> ");
            string? line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line) ||
                line.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
                break;

            var r = client.Query(line.Trim());
            Print(r, line.Trim());
        }
    }

    // ── 출력 유틸 ───────────────────────────────────────────────
    private static void Print(EspecResult r, string label)
    {
        if (r.Ok)
            Console.WriteLine($"[OK]  {label,-16} : {r.Raw}");
        else
            Console.WriteLine($"[NA]  {label,-16} : {(string.IsNullOrEmpty(r.Raw) ? r.Error : r.Raw)}");
    }

    private static string[] SplitCsv(string s) =>
        s.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static void PrintHeader()
    {
        Console.WriteLine("==================================================");
        Console.WriteLine(" ESPEC Platinous J series 온·습도 테스트 콘솔 (Ethernet)");
        Console.WriteLine("==================================================");
    }
}
