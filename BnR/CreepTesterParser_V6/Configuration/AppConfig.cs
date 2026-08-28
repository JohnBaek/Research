using System.Text.Json;

namespace CreepTesterParser_V6.Configuration;

/// <summary>
/// appsettings.json (+ appsettings.{환경}.json) 에서 설정을 읽는다.
/// 외부 라이브러리 없이 System.Text.Json 만 사용.
/// 환경은 DOTNET_ENVIRONMENT 로 결정하며, 미지정 시 Development(맥 개발 기준).
/// 실제 운영 PC(윈도우)에서는 DOTNET_ENVIRONMENT=Production 으로 실행.
/// </summary>
public sealed class AppConfig
{
    public required string Environment { get; init; }
    public required string SettingFilePath { get; init; }

    /// <summary>마지막 데이터가 이 시간(초) 이내면 '동작중'. 미지정 시 1800초(30분).</summary>
    public int RunningFreshnessSeconds { get; init; } = 1800;

    public static AppConfig Load()
    {
        var baseDir = AppContext.BaseDirectory;
        var env = System.Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";

        // base 먼저, 그 위에 환경별 파일로 덮어쓰기.
        var merged = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        MergeFrom(merged, Path.Combine(baseDir, "appsettings.json"));
        MergeFrom(merged, Path.Combine(baseDir, $"appsettings.{env}.json"));

        if (!merged.TryGetValue("SettingFilePath", out var settingPath) || string.IsNullOrWhiteSpace(settingPath))
            throw new InvalidOperationException("appsettings.json 에 SettingFilePath 값이 없습니다.");

        var freshness = merged.TryGetValue("RunningFreshnessSeconds", out var fs) && int.TryParse(fs, out var f) && f > 0
            ? f : 1800;

        return new AppConfig
        {
            Environment = env,
            SettingFilePath = settingPath,
            RunningFreshnessSeconds = freshness,
        };
    }

    private static void MergeFrom(Dictionary<string, string> target, string path)
    {
        if (!File.Exists(path)) return; // 환경별 파일은 없을 수 있음(선택)

        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            target[prop.Name] = prop.Value.ValueKind switch
            {
                JsonValueKind.String => prop.Value.GetString() ?? "",
                JsonValueKind.Number => prop.Value.GetRawText(),
                JsonValueKind.True or JsonValueKind.False => prop.Value.GetRawText(),
                _ => target.TryGetValue(prop.Name, out var existing) ? existing : "",
            };
        }
    }
}
