using AwesomeAssertions;
using DotNet_10.Features;

namespace DotNet_10.Tests;

// [Feature03] 널 조건부 대입 — null이면 스킵, 아니면 대입, 오른쪽식 지연평가 확인
public class NullConditionalAssignmentTests
{
    [Fact]
    public void 대상이_null이면_예외없이_그냥_넘어간다()
    {
        Order? order = null;

        var act = () => NullConditionalAssignmentDemo.UpdateQuantity(order, 5);

        act.Should().NotThrow();
    }

    [Fact]
    public void 대상이_존재하면_값이_갱신된다()
    {
        var order = new Order { Quantity = 1 };

        NullConditionalAssignmentDemo.UpdateQuantity(order, 42);

        order.Quantity.Should().Be(42);
    }

    [Fact]
    public void 대상이_null이면_오른쪽_식은_평가되지_않는다()
    {
        Order? order = null;
        var sideEffectCalled = false;

        NullConditionalAssignmentDemo.UpdateQuantity(order, () =>
        {
            sideEffectCalled = true;
            return 1;
        });

        sideEffectCalled.Should().BeFalse();
    }
}
