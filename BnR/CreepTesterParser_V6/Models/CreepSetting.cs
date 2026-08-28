namespace CreepTesterParser_V6.Models;

/// <summary>
/// Setting.ini 전체를 해석한 결과 모델.
/// </summary>
public sealed class CreepSetting
{
    /// <summary>
    /// 실제 읽은 Setting.ini 경로.
    /// </summary>
    public required string SourcePath { get; init; }
    
    /// <summary>
    /// 활성 채널 수. [InputMode_Set] USE_CH.
    /// </summary>
    public int UseChannelCount { get; init; }
    
    /// <summary>
    /// 하중 단위. [SETUP] UNIT1 (예: kg).
    /// </summary>
    public string? LoadUnit { get; init; }
    
    /// <summary>
    /// 레버비. [InputMode_Set] RATIO.
    /// </summary>
    public string? LeverRatio { get; init; }
    
    /// <summary>
    /// 통신 포인트 사용 여부/모드 등 기타 참고값.
    /// </summary>
    public string? ComMode { get; init; }
    public string? ComPort { get; init; }
    public string? StartNumber { get; init; }
    
    /// <summary>
    /// 활성 채널(1..UseChannelCount) 정보.
    /// </summary>
    public IReadOnlyList<ChannelInfo> ActiveChannels { get; init; } = [];
    
    /// <summary>
    /// 활성 채널의 저장 폴더 목록(중복 제거).
    /// </summary>
    public IReadOnlyList<string> DistinctFolders =>
        ActiveChannels
            .Select(c => c.Folder)
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .Select(f => f!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            .ToList();
}
