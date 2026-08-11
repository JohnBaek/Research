# DotNet_10 — C# 14 / .NET 10 신기능 정리

.NET 10 SDK와 함께 릴리스된 **C# 14 언어 기능**과 **런타임/BCL 신규 API**를,
각각 실행/검증 가능한 형태로 정리한 공부용 프로젝트입니다.

- `DotNet_10.Features` — 기능별 데모 코드 (상세 주석)
- `DotNet_10.Tests` — 각 기능이 실제로 동작함을 증명하는 테스트

## 테스트 스택

| 항목 | 선택 | 비고 |
|------|------|------|
| 테스트 프레임워크 | **xUnit v3** (`xunit.v3`) | 최신 세대 |
| 어서션 | **AwesomeAssertions** | FluentAssertions 7 포크(MIT). `.Should()` 문법 동일 |

> **왜 AwesomeAssertions?** FluentAssertions는 v8부터(2025) 상용 라이선스로
> 전환됐습니다. AwesomeAssertions는 마지막 무료 버전(v7)을 그대로 포크한
> MIT 라이선스 프로젝트라, 코드 변경 없이 FluentAssertions 문법을 계속 쓸 수 있습니다.
> (대안: Shouldly — 무료지만 `result.ShouldBe(x)` 형태로 문법이 다름)

## 실행

```bash
# 솔루션 루트(Research)에서
dotnet test DotNet_10/DotNet_10.Tests
```

## 다루는 기능

### C# 14 언어 기능
| # | 기능 | 한 줄 요약 |
|---|------|-----------|
| 01 | `field` 키워드 | 접근자 안에서 backing field를 `field`로 직접 접근 (필드 선언 생략) |
| 02 | 확장 멤버 | 확장 "프로퍼티"·정적 멤버까지 가능 (`extension(T x) { ... }`) |
| 03 | 널 조건부 대입 | `target?.Member = value;` — null이면 대입 스킵 |
| 04 | 언바운드 제네릭 `nameof` | `nameof(List<>)` → `"List"` |
| 05 | 수식어 붙은 람다 매개변수 | `(text, out result) => ...` — 타입 생략 + 수식어 유지 |
| 06 | 사용자 정의 복합 대입 연산자 | `public void operator +=(...)` — 제자리(in-place) 수정 |

### 런타임 / BCL 신규 API
| # | 기능 | 한 줄 요약 |
|---|------|-----------|
| 07 | LINQ `LeftJoin` / `RightJoin` | 표준 LINQ에 외부 조인 추가. 매칭 없는 쪽은 `null` |
| 08 | LINQ `Shuffle` | 시퀀스를 무작위로 섞은 새 시퀀스 (원본 불변) |
| 09 | `JsonSerializerOptions.Strict` | 매핑 안 되는 속성·대소문자 불일치를 예외로 처리하는 엄격 프리셋 |

각 기능의 상세 설명은 `DotNet_10.Features/Feature0*.cs` 파일 상단 주석을 참고하세요.

### 01. `field` 키워드
```csharp
public double Celsius
{
    get => field;
    set
    {
        if (value < -273.15) throw new ArgumentOutOfRangeException(nameof(value));
        field = value;   // ← 별도 private 필드 선언 없이 backing field 접근
    }
}
```

### 03. 널 조건부 대입 (before / after)
```csharp
// before (C# 13)
if (order is not null) order.Quantity = value;
// after (C# 14)
order?.Quantity = value;   // order가 null이면 오른쪽 식도 평가되지 않음
```

### 05. 수식어 붙은 람다 매개변수 (before / after)
```csharp
// before: 수식어를 쓰려면 타입도 필수
TryParse<int> p = (string text, out int result) => int.TryParse(text, out result);
// after: 타입 생략, 수식어만
TryParse<int> p = (text, out result) => int.TryParse(text, out result);
```

### 07. LINQ LeftJoin (before / after)
```csharp
// before: GroupJoin + SelectMany + DefaultIfEmpty 조합
var q = users.GroupJoin(purchases, u => u.Id, p => p.UserId, (u, ps) => (u, ps))
             .SelectMany(x => x.ps.DefaultIfEmpty(), (x, p) => (x.u.Name, p?.Product));
// after: 표준 LeftJoin
var q = users.LeftJoin(purchases, u => u.Id, p => p.UserId, (u, p) => (u.Name, p?.Product));
```

### 09. JsonSerializerOptions.Strict
```csharp
// 기본 옵션: 여분 속성 "Unknown"을 조용히 무시 → 통과
JsonSerializer.Deserialize<AppConfig>(json);
// Strict: 매핑 안 되는 속성이 있으면 JsonException, 대소문자도 구분
JsonSerializer.Deserialize<AppConfig>(json, JsonSerializerOptions.Strict);
```
