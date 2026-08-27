namespace CreepTesterParser_V6.Models;

/// <summary>
/// 채널(챔버 1대) 단위 설정 정보. Setting.ini 의 접미사 번호 {n} 로 묶인 값들.
/// </summary>
public sealed class ChannelInfo
{
    public int Index { get; init; }

    /// <summary>장비 IP (채널↔챔버 고정 식별키). [InputMode_Set] IP{n}</summary>
    public string? Ip { get; init; }

    /// <summary>작업자. USERNAME{n}</summary>
    public string? UserName { get; init; }

    /// <summary>시험 시작일. EXPDATE{n}</summary>
    public string? ExpDate { get; init; }

    /// <summary>설정 하중(kgf, 레버비 반영값). EXPLOAD{n}</summary>
    public string? ExpLoad { get; init; }

    /// <summary>목표 온도. EXPTEMP{n}</summary>
    public string? ExpTemp { get; init; }

    /// <summary>표점거리 L0 (Strain 계산 기준). GAGELENGTH{n}</summary>
    public string? GageLength { get; init; }

    /// <summary>응력(MPa). STRESS{n}</summary>
    public string? Stress { get; init; }

    /// <summary>FILEPATH{n} (폴더)</summary>
    public string? FilePath { get; init; }

    /// <summary>FILENAME{n} (파일명)</summary>
    public string? FileName { get; init; }

    /// <summary>SAVEFILEPATH{n} (실제 저장 전체 경로). 데이터 파일 읽기의 기준.</summary>
    public string? SaveFilePath { get; init; }

    /// <summary>저장 컬럼 비트마스크. SAVE_CONTENTS{n} (예: 1111111111 / 11111100)</summary>
    public string? SaveContents { get; init; }

    /// <summary>표시/데이터 읽기에 사용할 전체 경로. SAVEFILEPATH 우선, 없으면 FILEPATH+FILENAME 조합.</summary>
    public string? ResolvedFullPath
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(SaveFilePath)) return SaveFilePath;
            if (!string.IsNullOrWhiteSpace(FilePath) && !string.IsNullOrWhiteSpace(FileName))
                return CombineWindows(FilePath!, FileName!);
            return null;
        }
    }

    /// <summary>전체 경로에서 폴더 부분(윈도우 경로 기준, 백슬래시 분리).</summary>
    public string? Folder
    {
        get
        {
            var full = ResolvedFullPath;
            if (string.IsNullOrWhiteSpace(full)) return null;
            var idx = full.LastIndexOf('\\');
            return idx > 0 ? full[..idx] : full;
        }
    }

    private static string CombineWindows(string folder, string file)
        => folder.EndsWith('\\') ? folder + file : folder + "\\" + file;
}
