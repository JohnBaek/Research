using System.Text.Json;

namespace DotNet_10.Features;

/// <summary>
/// [.NET 10 BCL] JsonSerializerOptions.Strict 프리셋
///
/// System.Text.Json은 기본적으로 "관대"하다 — JSON에 여분의 속성이 있어도
/// 조용히 무시하고 넘어간다. 그래서 오타나 스키마 불일치를 놓치기 쉬웠다.
///
/// .NET 10부터 JsonSerializerOptions.Strict 프리셋이 추가됐다. 이 프리셋은
/// 데이터 검증을 엄격하게 한다:
///   - 대상 타입에 매핑되지 않는 JSON 속성이 있으면 예외
///   - 프로퍼티 이름 대소문자를 구분 (Web 프리셋은 대소문자 무시)
///   - 중복 속성 거부 등
///
/// → 설정 파일/외부 계약(contract) 검증처럼 "조용한 무시"가 위험한 곳에 적합.
/// </summary>
public static class JsonStrictDemo
{
    /// <summary>역직렬화 대상 모델.</summary>
    public sealed class AppConfig
    {
        public string? Name { get; set; }
        public int Port { get; set; }
    }

    /// <summary>Strict 프리셋으로 역직렬화. 스키마에 안 맞으면 JsonException.</summary>
    public static AppConfig? DeserializeStrict(string json) =>
        JsonSerializer.Deserialize<AppConfig>(json, JsonSerializerOptions.Strict);

    /// <summary>기본 옵션으로 역직렬화(대조군). 여분 속성은 조용히 무시된다.</summary>
    public static AppConfig? DeserializeDefault(string json) =>
        JsonSerializer.Deserialize<AppConfig>(json);
}
