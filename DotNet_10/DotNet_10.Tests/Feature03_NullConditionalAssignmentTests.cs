using AwesomeAssertions;
using DotNet_10.Features;

namespace DotNet_10.Tests;

// [Feature03] Null-conditional assignment - skip on null, assign otherwise, lazy right side
public class NullConditionalAssignmentTests
{
    [Fact]
    public void When_target_is_null_it_just_passes_without_throwing()
    {
        Order? order = null;

        var act = () => NullConditionalAssignmentDemo.UpdateQuantity(order, 5);

        act.Should().NotThrow();
    }

    [Fact]
    public void When_target_exists_the_value_is_updated()
    {
        var order = new Order { Quantity = 1 };

        NullConditionalAssignmentDemo.UpdateQuantity(order, 42);

        order.Quantity.Should().Be(42);
    }

    [Fact]
    public void When_target_is_null_the_right_side_is_not_evaluated()
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
