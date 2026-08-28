using System.Text;

namespace CreepTesterParser_V6.Ini;

/// <summary>
/// 아주 단순한 INI 파서. Setting.ini 구조( [Section] 아래 KEY=VALUE )를 그대로 담는다.
/// - 섹션/키 모두 대소문자 무시
/// - 같은 키가 여러 섹션에 있을 수 있으므로 섹션 단위로 보관
/// - 파일 인코딩은 CP949(EUC-KR) 고정 (한글 작업자명/폴더명 때문)
/// </summary>
public sealed class IniDocument
{
    // section -> (key -> value)
    private readonly Dictionary<string, Dictionary<string, string>> _sections =
        new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, Dictionary<string, string>> Sections => _sections;

    /// <summary>
    /// Setting.ini 를 가져온다.
    /// </summary>
    /// <param name="path">Setting.ini 실제 파일경로</param>
    /// <param name="encoding">인코딩 정보 (보통 EUC-KR)</param>
    /// <returns></returns>
    public static IniDocument Load(string path, Encoding encoding)
    {
        var doc = new IniDocument();
        var current = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        doc._sections[""] = current; // 섹션 헤더 이전에 나오는 키를 위한 기본 버킷

        // R&B 프로그램이 이 파일에 시험 진행값([SAVE_TIME] 등)을 계속 되쓴다.
        // 절대 프로그램의 쓰기를 막지 않도록 읽기 전용 + 공유 모드로 연다.
        using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using StreamReader streamReader = new StreamReader(fileStream, encoding);

        // 파일의 모든 라인을 읽어온다.
        while (streamReader.ReadLine() is { } rawLine)
        {
            // 라인한줄을 읽어온다.
            string line = rawLine.Trim();
            
            // 내용이 없을경우 제외
            if (line.Length == 0) 
                continue;
            
            // ; 또는 # 로 시작하는 라인일경우 제외
            if (line.StartsWith(';') || line.StartsWith('#')) 
                continue; 

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                string name = line[1..^1].Trim();
                if (!doc._sections.TryGetValue(name, out current!))
                {
                    current = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    doc._sections[name] = current;
                }
                continue;
            }

            // '=' 가 포함된 라인인지?
            int eq = line.IndexOf('=');
            
            // key=value 형식이 아니면 skip
            if (eq < 0) 
                continue; 
            
            // 해당 라인에 해다하는 값을 읽어온다.
            string key = line[..eq].Trim();
            string value = line[(eq + 1)..].Trim();
            
            // 키가 없을경우 제외
            if (key.Length == 0) 
                continue;
            
            // 중복 키는 마지막 값 채택
            current[key] = value; 
        }

        return doc;
    }

    /// <summary>지정 섹션의 키 값. 없으면 null.</summary>
    public string? Get(string section, string key)
        => _sections.TryGetValue(section, out var s) && s.TryGetValue(key, out var v) ? v : null;

    /// <summary>섹션을 특정하지 않고 파일 전체에서 처음 발견되는 키 값. 없으면 null.</summary>
    public string? Find(string key)
    {
        foreach (var s in _sections.Values)
            if (s.TryGetValue(key, out var v)) return v;
        return null;
    }

    public IReadOnlyDictionary<string, string>? Section(string section)
        => _sections.TryGetValue(section, out var s) ? s : null;
}
