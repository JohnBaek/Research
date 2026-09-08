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
    /// <summary>
    /// 예: 260706_165628 → 2026-07-06 16:56:28
    /// </summary>
    private const string PcTimeFormat = "yyMMdd_HHmmss";  

    /// <summary>
    /// .CSV Reader 오브젝트
    /// </summary>
    private readonly CsvPeekReader _csv;

    /// <summary>
    /// 생성자
    /// </summary>
    /// <param name="csv"></param>
    public CsvActivitySource(CsvPeekReader csv) => _csv = csv;

    /// <summary>
    /// 마지막 행의 날짜와 Raw 번호 를 읽는다.
    /// </summary>
    /// <param name="setting"></param>
    /// <returns></returns>
    public IReadOnlyDictionary<int, DateTime?> GetLastActivity(CreepSetting setting)
    {
        Dictionary<int, DateTime?> map = new Dictionary<int, DateTime?>(setting.ActiveChannels.Count);
        foreach (var ch in setting.ActiveChannels)
        {
            DateTime? lastTime = null;
            string? path = ch.ResolvedFullPath;
            if (path is not null)
            {
                string? line = _csv.ReadLastLineOrNull(path);
                lastTime = ParsePcTime(FirstField(line));
            }
            map[ch.Index] = lastTime;
        }
        return map;
    }

    /// <summary>
    /// 현재 첫번째 Field 인지 여부를 반환 한다.
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    private static string? FirstField(string? line) => line?.Split(',', 2)[0].Trim();

    /// <summary>
    /// String 날짜 정보를 DateTime Nullable 로 반환 한다.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static DateTime? ParsePcTime(string? s)
        => DateTime.TryParseExact(s, PcTimeFormat, CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var dt) ? dt : null;
}
