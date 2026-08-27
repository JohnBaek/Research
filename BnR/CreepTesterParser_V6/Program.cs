using System.Text;
using CreepTesterParser_V6.Configuration;
using CreepTesterParser_V6.Models;
using CreepTesterParser_V6.Services;

namespace CreepTesterParser_V6;

internal static class Program
{
    private static void Main(string[] args)
    {
        // CP949(EUC-KR) 코드페이지 등록 (Setting.ini 한글 디코딩용) + 콘솔 한글 출력.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Console.OutputEncoding = Encoding.UTF8;

        var csv = new CsvPeekReader();

        // 테스트/스크립트용: --peek <csv경로> 로 특정 파일만 훑어보고 종료.
        if (args is ["--peek", var peekPath, ..])
        {
            PeekAndPrint(csv, peekPath);
            return;
        }

        AppConfig config;
        try
        {
            config = AppConfig.Load();
        }
        catch (Exception ex)
        {
            WriteError($"설정 로드 실패: {ex.Message}");
            return;
        }

        var reader = new SettingReader();

        // 입력이 리다이렉트된(비대화형) 환경에서는 ReadKey 를 쓸 수 없으므로
        // 설정 요약(메뉴 1)만 1회 출력하고 종료한다. (파이프/CI 실행 대비)
        if (Console.IsInputRedirected)
        {
            Console.WriteLine($"환경: {config.Environment} / Setting.ini: {config.SettingFilePath}");
            ShowSettingSummary(reader, config.SettingFilePath);
            return;
        }

        while (true)
        {
            PrintMenu(config);
            var key = Console.ReadKey(intercept: true).Key;
            Console.WriteLine();

            switch (key)
            {
                case ConsoleKey.D1 or ConsoleKey.NumPad1:
                    ShowSettingSummary(reader, config.SettingFilePath);
                    break;

                case ConsoleKey.D2 or ConsoleKey.NumPad2:
                    ShowFileRead(reader, csv, config.SettingFilePath);
                    break;

                case ConsoleKey.D0 or ConsoleKey.NumPad0 or ConsoleKey.Escape or ConsoleKey.Q:
                    Console.WriteLine("종료합니다.");
                    return;

                default:
                    WriteWarn("알 수 없는 입력입니다. 메뉴 번호를 눌러주세요.");
                    break;
            }

            Console.WriteLine();
            Console.WriteLine("계속하려면 아무 키나 누르세요...");
            Console.ReadKey(intercept: true);
        }
    }

    private static void PrintMenu(AppConfig config)
    {
        Console.Clear();
        Console.WriteLine("========================================");
        Console.WriteLine("  Creep Tester Parser V6");
        Console.WriteLine("========================================");
        Console.WriteLine($"  환경(Environment) : {config.Environment}");
        Console.WriteLine($"  Setting.ini       : {config.SettingFilePath}");
        Console.WriteLine($"  파일 존재 여부    : {(File.Exists(config.SettingFilePath) ? "있음" : "없음(!)")}");
        Console.WriteLine("----------------------------------------");
        Console.WriteLine("  [1] 설정 파일 읽기 (활성 채널/저장 폴더/주요 설정)");
        Console.WriteLine("  [2] 데이터 파일 읽기            (다음 단계)");
        Console.WriteLine("  [0] 종료 (Esc / Q)");
        Console.WriteLine("----------------------------------------");
        Console.Write("  선택: ");
    }

    private static void ShowSettingSummary(SettingReader reader, string path)
    {
        CreepSetting setting;
        try
        {
            setting = reader.Read(path);
        }
        catch (FileNotFoundException ex)
        {
            WriteError(ex.Message);
            return;
        }
        catch (Exception ex)
        {
            WriteError($"설정 파일 파싱 실패: {ex.Message}");
            return;
        }

        Console.WriteLine();
        Console.WriteLine("==================== 설정 요약 ====================");
        Console.WriteLine($"  원본 파일     : {setting.SourcePath}");
        Console.WriteLine($"  활성 채널 수  : {setting.UseChannelCount} 대  (USE_CH)");
        Console.WriteLine($"  하중 단위     : {setting.LoadUnit ?? "-"}   (UNIT1)");
        Console.WriteLine($"  레버비(RATIO) : {setting.LeverRatio ?? "-"}");
        Console.WriteLine($"  통신 모드     : COM_MODE={setting.ComMode ?? "-"}, COMPORT={setting.ComPort ?? "-"}, START_NUMBER={setting.StartNumber ?? "-"}");
        Console.WriteLine("--------------------------------------------------");

        Console.WriteLine($"  [활성 채널 {setting.ActiveChannels.Count}개]");
        foreach (var ch in setting.ActiveChannels)
        {
            Console.WriteLine(
                $"  [CH{ch.Index:D2}] IP={ch.Ip ?? "-",-15} " +
                $"온도={ch.ExpTemp ?? "-",-5} 하중={ch.ExpLoad ?? "-",-10} " +
                $"L0={ch.GageLength ?? "-",-6} 작업자={ch.UserName ?? "-"}  시작={ch.ExpDate ?? "-"}");
            Console.WriteLine($"          파일: {ch.ResolvedFullPath ?? "(경로 없음)"}");
        }

        Console.WriteLine("--------------------------------------------------");
        var folders = setting.DistinctFolders;
        Console.WriteLine($"  [저장 폴더 {folders.Count}곳]");
        foreach (var folder in folders)
            Console.WriteLine($"    - {folder}");
        Console.WriteLine("==================================================");
    }

    private static void ShowFileRead(SettingReader reader, CsvPeekReader csv, string settingPath)
    {
        CreepSetting setting;
        try
        {
            setting = reader.Read(settingPath);
        }
        catch (FileNotFoundException ex)
        {
            WriteError(ex.Message);
            return;
        }
        catch (Exception ex)
        {
            WriteError($"설정 파일 파싱 실패: {ex.Message}");
            return;
        }

        if (setting.ActiveChannels.Count == 0)
        {
            WriteWarn("활성 채널이 없습니다.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine("==================== 파일 목록 ====================");
        foreach (var ch in setting.ActiveChannels)
        {
            var full = ch.ResolvedFullPath;
            var exists = full is not null && File.Exists(full);
            var mark = full is null ? "[경로없음]" : exists ? "[있음]" : "[없음]  ";
            var name = full is null ? "-" : GetWindowsFileName(full);
            Console.WriteLine($"  {ch.Index,2}) {mark} CH{ch.Index:D2}  {name}   (IP={ch.Ip ?? "-"})");
        }
        Console.WriteLine("--------------------------------------------------");
        Console.Write("  읽을 채널 번호 입력 (취소: Enter): ");

        var input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("취소되었습니다.");
            return;
        }

        if (!int.TryParse(input.Trim(), out var index))
        {
            WriteWarn("숫자를 입력해주세요.");
            return;
        }

        var selected = setting.ActiveChannels.FirstOrDefault(c => c.Index == index);
        if (selected is null)
        {
            WriteWarn($"{index}번 채널이 목록에 없습니다.");
            return;
        }

        var path = selected.ResolvedFullPath;
        if (path is null)
        {
            WriteError($"CH{selected.Index:D2} 에 저장 경로가 없습니다.");
            return;
        }

        PeekAndPrint(csv, path);
    }

    private static void PeekAndPrint(CsvPeekReader csv, string path)
    {
        CsvPeek peek;
        try
        {
            peek = csv.Read(path);
        }
        catch (FileNotFoundException ex)
        {
            WriteError(ex.Message);
            return;
        }
        catch (Exception ex)
        {
            WriteError($"CSV 읽기 실패: {ex.Message}");
            return;
        }

        Console.WriteLine();
        Console.WriteLine("==================== 파일 미리보기 ====================");
        Console.WriteLine($"  경로     : {peek.Path}");
        Console.WriteLine($"  크기     : {FormatSize(peek.SizeBytes)}");
        Console.WriteLine($"  최종수정 : {peek.LastWriteTime:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine("------------------------------------------------------");
        Console.WriteLine($"  [헤더 ] {peek.Header ?? "(없음)"}");
        if (peek.HasData)
        {
            Console.WriteLine($"  [첫 행] {peek.FirstDataLine}");
            Console.WriteLine($"  [끝 행] {peek.LastDataLine}");
        }
        else
        {
            WriteWarn("  데이터 행이 없습니다 (헤더만 존재).");
        }
        Console.WriteLine("======================================================");
    }

    private static string GetWindowsFileName(string windowsPath)
    {
        var idx = windowsPath.LastIndexOf('\\');
        return idx >= 0 && idx < windowsPath.Length - 1 ? windowsPath[(idx + 1)..] : windowsPath;
    }

    private static string FormatSize(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB"];
        double size = bytes;
        var u = 0;
        while (size >= 1024 && u < units.Length - 1) { size /= 1024; u++; }
        return u == 0 ? $"{bytes} B" : $"{size:0.##} {units[u]} ({bytes:N0} bytes)";
    }

    private static void WriteError(string message)
    {
        var prev = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[오류] {message}");
        Console.ForegroundColor = prev;
    }

    private static void WriteWarn(string message)
    {
        var prev = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(message);
        Console.ForegroundColor = prev;
    }
}
