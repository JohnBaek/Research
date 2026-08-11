using AwesomeAssertions;
using DotNet_10.Features;

namespace DotNet_10.Tests;

// [Feature04] 언바운드 제네릭 nameof — 타입 인자 없이 이름만 반환되는지 확인
public class UnboundGenericNameofTests
{
    [Fact]
    public void List_언바운드는_List_이름을_준다()
    {
        UnboundGenericNameofDemo.ListName.Should().Be("List");
    }

    [Fact]
    public void Dictionary_언바운드는_Dictionary_이름을_준다()
    {
        UnboundGenericNameofDemo.DictionaryName.Should().Be("Dictionary");
    }
}
