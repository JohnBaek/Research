using System.Text;

namespace CreepTesterParser_V6.Services;

/// <summary>CSV 앞/뒤만 훑어본 결과.</summary>
public sealed record CsvPeek(
    string Path,
    long SizeBytes,
    DateTime LastWriteTime,
    string? Header,
    string? FirstDataLine,
    string? LastDataLine)
{
    public bool HasData => FirstDataLine is not null;
}

/// <summary>
/// CSV 파일에서 헤더 + 첫 데이터 행 + 마지막 행만 읽는다.
/// - 파일이 커도 전체를 로드하지 않는다(마지막 행은 끝에서 역방향으로 스캔).
/// - 운영 프로그램이 append 중일 수 있어 FileShare.ReadWrite 로 연다(간섭 방지).
/// - 인코딩은 CP949(EUC-KR) 고정 (R&B 프로그램 저장 기준).
/// </summary>
public sealed class CsvPeekReader
{
    private static readonly Encoding Cp949 = Encoding.GetEncoding(949);

    private const FileShare Share = FileShare.ReadWrite | FileShare.Delete;

    public CsvPeek Read(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"CSV 파일을 찾을 수 없습니다: {path}", path);

        var fi = new FileInfo(path);

        string? header;
        string? first;
        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, Share))
        using (var sr = new StreamReader(fs, Cp949))
        {
            header = ReadNextNonEmpty(sr);
            first = ReadNextNonEmpty(sr);
        }

        // 데이터 행이 하나도 없으면(헤더만) 마지막 행은 의미 없음.
        var last = first is null ? null : ReadLastNonEmptyLine(path);

        return new CsvPeek(path, fi.Length, fi.LastWriteTime, header, first, last);
    }

    /// <summary>파일이 없으면 null, 있으면 마지막 비어있지 않은 한 줄. (상태 감시용 경량 읽기)</summary>
    public string? ReadLastLineOrNull(string path)
        => File.Exists(path) ? ReadLastNonEmptyLine(path) : null;

    private static string? ReadNextNonEmpty(StreamReader sr)
    {
        string? line;
        while ((line = sr.ReadLine()) is not null)
        {
            if (line.Trim().Length > 0) return line;
        }
        return null;
    }

    /// <summary>
    /// 파일 끝에서부터 역방향으로 읽어 마지막 비어있지 않은 한 줄을 반환.
    /// CP949 의 트레일 바이트 범위(0x41-0xFE)에는 0x0A/0x0D 가 없으므로 개행 바이트 기준 분리는 안전.
    /// </summary>
    private static string? ReadLastNonEmptyLine(string path)
    {
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, Share);
        var len = fs.Length;
        if (len == 0) return null;

        const int chunkSize = 8192;
        var buffer = new byte[chunkSize];
        var lineBytes = new List<byte>(256); // 마지막 줄 바이트(역순으로 수집)
        var pos = len;
        var seenContent = false;

        while (pos > 0)
        {
            var toRead = (int)Math.Min(chunkSize, pos);
            pos -= toRead;
            fs.Seek(pos, SeekOrigin.Begin);
            fs.ReadExactly(buffer, 0, toRead);

            for (var i = toRead - 1; i >= 0; i--)
            {
                var b = buffer[i];
                if (b == (byte)'\n' || b == (byte)'\r')
                {
                    if (seenContent) // 마지막 줄의 시작 경계에 도달
                    {
                        lineBytes.Reverse();
                        return Cp949.GetString(lineBytes.ToArray());
                    }
                    continue; // 끝쪽 개행은 건너뜀
                }

                seenContent = true;
                lineBytes.Add(b);
            }
        }

        if (!seenContent) return null;
        lineBytes.Reverse();
        return Cp949.GetString(lineBytes.ToArray());
    }
}
