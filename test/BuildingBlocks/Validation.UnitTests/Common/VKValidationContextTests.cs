using System;
using System.Threading;
using Moq;
using VK.Blocks.Core;
using VK.Blocks.Testing;
using VK.Blocks.Validation;
using Xunit;

namespace VK.Blocks.Validation.UnitTests.Common;

public sealed class VKValidationContextTests : VKUnitTestBase<VKValidationContext>
{
    private sealed record SampleModel(string Name);

    [Fact]
    public void Context_WithExplicitValues_ShouldRetainValues()
    {
        var context = new VKValidationContext(
            cancellationToken: CancellationToken.None,
            tenantId: "tenant-123",
            userId: "user-456");

        Assert.Equal("tenant-123", context.TenantId);
        Assert.Equal("user-456", context.UserId);
    }

    [Fact]
    public void GenericContext_ShouldWrapModelAndContext()
    {
        var model = new SampleModel("test");
        var baseContext = new VKValidationContext(tenantId: "tenant-1");
        var typedContext = new VKValidationContext<SampleModel>(model, baseContext);

        Assert.Same(model, typedContext.Model);
        Assert.Equal("tenant-1", typedContext.Context.TenantId);
    }
}
