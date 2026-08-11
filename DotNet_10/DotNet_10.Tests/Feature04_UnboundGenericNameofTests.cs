using AwesomeAssertions;
using DotNet_10.Features;

namespace DotNet_10.Tests;

// [Feature04] Unbound generic nameof - check the name comes back without type args
public class UnboundGenericNameofTests
{
    [Fact]
    public void Unbound_List_gives_the_name_List()
    {
        UnboundGenericNameofDemo.ListName.Should().Be("List");
    }

    [Fact]
    public void Unbound_Dictionary_gives_the_name_Dictionary()
    {
        UnboundGenericNameofDemo.DictionaryName.Should().Be("Dictionary");
    }
}
