using System.Globalization;
using CreepTesterParser_V6.Models;

namespace CreepTesterParser_V6.Services;

/// <summary>
/// 각 활성 채널의 CSV 마지막 행에서 첫 컬럼 PC_TIME(YYMMDD_HHMMSS)을 읽어
/// "마지막 데이터 기록 시각"을 채널별로 반환한다. (실시간 동작 판정의 근거)
/// 파일이 없거나 파싱 실패면 null.
/// </summary>
public sealed class CsvActivitySource
{
    private const string PcTimeFormat = "yyMMdd_HHmmss"; // 예: 260706_165628 → 2026-07-06 16:56:28

    private readonly CsvPeekReader _csv;

    public CsvActivitySource(CsvPeekReader csv) => _csv = csv;

    public IReadOnlyDictionary<int, DateTime?> GetLastActivity(CreepSetting setting)
    {
        var map = new Dictionary<int, DateTime?>(setting.ActiveChannels.Count);
        foreach (var ch in setting.ActiveChannels)
        {
            DateTime? lastTime = null;
            var path = ch.ResolvedFullPath;
            if (path is not null)
            {
                var line = _csv.ReadLastLineOrNull(path);
                lastTime = ParsePcTime(FirstField(line));
            }
            map[ch.Index] = lastTime;
        }
        return map;
    }

    private static string? FirstField(string? line)
        => line is null ? null : line.Split(',', 2)[0].Trim();

    public static DateTime? ParsePcTime(string? s)
        => DateTime.TryParseExact(s, PcTimeFormat, CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var dt) ? dt : null;
}
