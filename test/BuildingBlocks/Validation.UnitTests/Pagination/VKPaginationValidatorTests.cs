using VK.Blocks.Testing;
using VK.Blocks.Validation;
using Xunit;

namespace VK.Blocks.Validation.UnitTests.Pagination;

public sealed class VKPaginationValidatorTests : VKUnitTestBase<VKPaginationValidatorTests>
{

    [Theory]
    [InlineData(1, 10)]
    [InlineData(5, 50)]
    public void ValidateOffsetPagination_WithValidParams_ShouldSucceed(int pageNumber, int pageSize)
    {
        var result = VKPaginationValidator.ValidateOffsetPagination(pageNumber, pageSize);
        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    public void ValidateOffsetPagination_WithInvalidPageNumber_ShouldFail(int pageNumber, int pageSize)
    {
        var result = VKPaginationValidator.ValidateOffsetPagination(pageNumber, pageSize);
        Assert.True(result.IsFailure);
        Assert.Equal(VKPaginationErrors.InvalidPageNumber.Code, result.FirstError.Code);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(1, -5)]
    public void ValidateOffsetPagination_WithInvalidPageSize_ShouldFail(int pageNumber, int pageSize)
    {
        var result = VKPaginationValidator.ValidateOffsetPagination(pageNumber, pageSize);
        Assert.True(result.IsFailure);
        Assert.Equal(VKPaginationErrors.InvalidPageSize.Code, result.FirstError.Code);
    }
}
