using VK.Blocks.Validation.Constants;

namespace VK.Blocks.Validation.UnitTests;

public class PaginationValidatorTests
{
    [Theory]
    [InlineData(1, 10)]
    [InlineData(100, 100)]
    public void ValidateOffsetPagination_ShouldReturnSuccess_WhenParametersAreValid(int pageNumber, int pageSize)
    {
        // Act
        var result = PaginationValidator.ValidateOffsetPagination(pageNumber, pageSize);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    public void ValidateOffsetPagination_ShouldReturnFailure_WhenPageNumberIsInvalid(int pageNumber, int pageSize)
    {
        // Act
        var result = PaginationValidator.ValidateOffsetPagination(pageNumber, pageSize);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Pagination.InvalidPageNumber");
        result.Error.Description.Should().Be(PaginationConstants.ErrorMessages.PageNumberMustBePositive);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    public void ValidateOffsetPagination_ShouldReturnFailure_WhenPageSizeIsInvalid(int pageNumber, int pageSize)
    {
        // Act
        var result = PaginationValidator.ValidateOffsetPagination(pageNumber, pageSize);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Pagination.InvalidPageSize");
        result.Error.Description.Should().Be(PaginationConstants.ErrorMessages.PageSizeMustBePositive);
    }

    [Fact]
    public void ValidateOffsetPagination_ShouldReturnFailure_WhenPageSizeExceedsLimit()
    {
        // Arrange
        int pageSize = PaginationConstants.PerformanceGuard.MaxPageSize + 1;

        // Act
        var result = PaginationValidator.ValidateOffsetPagination(1, pageSize);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Pagination.OverPageSize");
    }

    [Fact]
    public void ValidateOffsetPagination_ShouldReturnFailure_WhenOffsetExceedsLimit()
    {
        // Arrange
        int pageSize = 100;
        int pageNumber = (PaginationConstants.PerformanceGuard.MaxOffsetLimit / pageSize) + 2;

        // Act
        var result = PaginationValidator.ValidateOffsetPagination(pageNumber, pageSize);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Pagination.OverOffsetLimit");
    }

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    public void ValidateCursorPagination_ShouldReturnSuccess_WhenParametersAreValid(int pageSize)
    {
        // Act
        var result = PaginationValidator.ValidateCursorPagination(pageSize);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateCursorPagination_ShouldReturnFailure_WhenPageSizeExceedsLimit()
    {
        // Arrange
        int pageSize = PaginationConstants.PerformanceGuard.MaxCursorLimit + 1;

        // Act
        var result = PaginationValidator.ValidateCursorPagination(pageSize);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Pagination.OverPageSize");
    }
}
