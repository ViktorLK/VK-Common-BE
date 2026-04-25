namespace VK.Blocks.Core.UnitTests.Contracts;

public class VKErrorDetailTests
{
    [Fact]
    public void VKErrorDetail_Properties_ShouldBeAssignable()
    {
        // Act
        var detail = new VKErrorDetail
        {
            Code = "Code",
            Detail = "Detail"
        };

        // Assert
        detail.Code.Should().Be("Code");
        detail.Detail.Should().Be("Detail");
    }
}
