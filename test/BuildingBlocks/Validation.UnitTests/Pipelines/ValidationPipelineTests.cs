using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using VK.Blocks.Testing;
using VK.Blocks.Testing.Core.Assertions;
using VK.Blocks.Validation;
using VK.Blocks.Validation.Pipeline.Internal;
using Xunit;

namespace VK.Blocks.Validation.UnitTests.Pipelines;

public sealed class ValidationPipelineTests : VKUnitTestBase<ValidationPipelineTests>
{

    private sealed record TestModel(string Name);

    [Fact]
    public async Task ValidateAsync_WithMultipleValidators_ShouldExecuteAndAggregateErrors()
    {
        var mockValidator1 = new Mock<IVKValidator>();
        mockValidator1.Setup(v => v.CanValidate(It.IsAny<object>())).Returns(true);
        mockValidator1.Setup(v => v.ValidateAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKValidationResult.Failure("Field1", "Error 1"));

        var mockValidator2 = new Mock<IVKValidator>();
        mockValidator2.Setup(v => v.CanValidate(It.IsAny<object>())).Returns(true);
        mockValidator2.Setup(v => v.ValidateAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKValidationResult.Failure("Field2", "Error 2"));

        var options = Options.Create(new VKValidationOptions());
        var pipeline = new ValidationPipeline(
            new[] { mockValidator1.Object, mockValidator2.Object },
            options,
            NullLogger<ValidationPipeline>.Instance);

        var result = await pipeline.ValidateAsync(new TestModel("Sample"));

        result.ShouldBeInvalid();
        Assert.Equal(2, result.Errors.Count);
        result.ShouldHaveErrorFor("Field1");
        result.ShouldHaveErrorFor("Field2");
    }

    [Fact]
    public async Task ValidateAsync_WithShortCircuit_ShouldStopOnFirstFailure()
    {
        var mockValidator1 = new Mock<IVKValidator>();
        mockValidator1.Setup(v => v.CanValidate(It.IsAny<object>())).Returns(true);
        mockValidator1.Setup(v => v.ValidateAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKValidationResult.Failure("Field1", "Error 1"));

        var mockValidator2 = new Mock<IVKValidator>();
        mockValidator2.Setup(v => v.CanValidate(It.IsAny<object>())).Returns(true);
        mockValidator2.Setup(v => v.ValidateAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VKValidationResult.Failure("Field2", "Error 2"));

        var options = Options.Create(new VKValidationOptions { ShortCircuitOnFirstFailure = true });
        var pipeline = new ValidationPipeline(
            new[] { mockValidator1.Object, mockValidator2.Object },
            options,
            NullLogger<ValidationPipeline>.Instance);

        var result = await pipeline.ValidateAsync(new TestModel("Sample"));

        result.ShouldBeInvalid();
        Assert.Single(result.Errors);
        result.ShouldHaveErrorFor("Field1");
        mockValidator2.Verify(v => v.ValidateAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
