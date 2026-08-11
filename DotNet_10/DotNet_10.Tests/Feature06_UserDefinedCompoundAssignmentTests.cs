using AwesomeAssertions;
using DotNet_10.Features;

namespace DotNet_10.Tests;

// [Feature06] 사용자 정의 복합 대입 연산자 — += / -= 가 제자리 수정하는지 확인
public class UserDefinedCompoundAssignmentTests
{
    [Fact]
    public void PlusEquals_가_제자리에서_누적한다()
    {
        var acc = new Accumulator(10);

        acc += 5;

        acc.Total.Should().Be(15);
    }

    [Fact]
    public void MinusEquals_가_제자리에서_감소시킨다()
    {
        var acc = new Accumulator(10);

        acc -= 3;

        acc.Total.Should().Be(7);
    }
}
