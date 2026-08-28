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

    public static IniDocument Load(string path, Encoding encoding)
    {
        var doc = new IniDocument();
        var current = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        doc._sections[""] = current; // 섹션 헤더 이전에 나오는 키를 위한 기본 버킷

        // R&B 프로그램이 이 파일에 시험 진행값([SAVE_TIME] 등)을 계속 되쓴다.
        // 절대 프로그램의 쓰기를 막지 않도록 읽기 전용 + 공유 모드로 연다.
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete);
        using var sr = new StreamReader(fs, encoding);

        string? rawLine;
        while ((rawLine = sr.ReadLine()) is not null)
        {
            var line = rawLine.Trim();
            if (line.Length == 0) continue;
            if (line.StartsWith(';') || line.StartsWith('#')) continue; // 주석

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                var name = line[1..^1].Trim();
                if (!doc._sections.TryGetValue(name, out current!))
                {
                    current = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    doc._sections[name] = current;
                }
                continue;
            }

            var eq = line.IndexOf('=');
            if (eq < 0) continue; // key=value 형식이 아니면 skip
            var key = line[..eq].Trim();
            var value = line[(eq + 1)..].Trim();
            if (key.Length == 0) continue;
            current[key] = value; // 중복 키는 마지막 값 채택
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
