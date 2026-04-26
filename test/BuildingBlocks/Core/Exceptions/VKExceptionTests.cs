namespace VK.Blocks.Core.UnitTests.Exceptions;

public class VKExceptionTests
{
    [Fact]
    public void VKConflictException_ShouldSetCorrectProperties()
    {
        var ex = new VKConflictException("Conflict occurred");
        ex.Message.Should().Be("Conflict occurred");
        ex.Code.Should().Be("Core.Conflict");
        ex.StatusCode.Should().Be(409);
    }

    [Fact]
    public void VKForbiddenException_ShouldSetCorrectProperties()
    {
        var ex = new VKForbiddenException("Access denied");
        ex.Message.Should().Be("Access denied");
        ex.Code.Should().Be("Core.Forbidden");
        ex.StatusCode.Should().Be(403);
    }

    [Fact]
    public void VKNotFoundException_ShouldSetCorrectProperties()
    {
        var ex = new VKNotFoundException("User not found");
        ex.Message.Should().Be("User not found");
        ex.Code.Should().Be("Core.NotFound");
        ex.StatusCode.Should().Be(404);
    }

    [Fact]
    public void VKUnauthorizedException_ShouldSetCorrectProperties()
    {
        var ex = new VKUnauthorizedException("Unauthorized access");
        ex.Message.Should().Be("Unauthorized access");
        ex.Code.Should().Be("Core.Unauthorized");
        ex.StatusCode.Should().Be(401);
    }

    [Fact]
    public void VKValidationException_ShouldSetCorrectProperties()
    {
        var ex = new VKValidationException("Validation failed");
        ex.Message.Should().Be("Validation failed");
        ex.Code.Should().Be("Core.ValidationError");
        ex.StatusCode.Should().Be(400);
    }

    [Fact]
    public void VKConflictException_Duplicate_ShouldSetReason()
    {
        var ex = VKConflictException.Duplicate("User", "123");
        ex.Message.Should().Contain("User").And.Contain("123");
        ex.Reason.Should().Be("Duplicate");
    }

    [Fact]
    public void VKDependencyException_MissingDependency_ShouldSetDetails()
    {
        var ex = VKDependencyException.MissingDependency("Auth", "App");
        ex.Message.Should().Contain("App").And.Contain("Auth");
        ex.RequiredBlock.Should().Be("Auth");
        ex.DependentBlock.Should().Be("App");
    }

    [Fact]
    public void VKForbiddenException_ForPermission_ShouldSetRequiredPermission()
    {
        var ex = VKForbiddenException.ForPermission("Read");
        ex.Message.Should().Contain("Read");
        ex.RequiredPermission.Should().Be("Read");
    }

    [Fact]
    public void VKNotFoundException_ForEntity_ShouldSetDetails()
    {
        var ex = VKNotFoundException.ForEntity("User", "123");
        ex.Message.Should().Contain("User").And.Contain("123");
        ex.EntityType.Should().Be("User");
        ex.EntityId.Should().Be("123");
    }

    [Fact]
    public void VKValidationException_FromDictionary_ShouldSetErrors()
    {
        var errors = new Dictionary<string, string[]> { { "Email", ["Invalid"] } };
        var ex = new VKValidationException(errors);
        ex.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void VKDependencyException_MissingSection_ShouldSetSectionName()
    {
        var ex = VKDependencyException.MissingSection("VKBlocks:Web");
        ex.Message.Should().Contain("VKBlocks:Web");
        ex.SectionName.Should().Be("VKBlocks:Web");
    }

    [Fact]
    public void VKDependencyException_CircularDependency_ShouldSetCyclePath()
    {
        var ex = VKDependencyException.CircularDependency("A", ["A", "B", "A"]);
        ex.Message.Should().Contain("A -> B -> A");
        ex.CyclePath.Should().Be("A -> B -> A");
    }

    [Fact]
    public void VKDependencyException_FeatureConflict_ShouldSetFeatures()
    {
        var ex = VKDependencyException.FeatureConflict("Auth", "Guest", "Web");
        ex.Message.Should().Contain("Auth").And.Contain("Guest").And.Contain("Web");
        ex.FeatureA.Should().Be("Auth");
        ex.FeatureB.Should().Be("Guest");
    }

    [Fact]
    public void VKDependencyException_RequiredOptionMissing_ShouldSetOptionName()
    {
        var ex = VKDependencyException.RequiredOptionMissing("ApiKey", "Required for auth");
        ex.Message.Should().Contain("ApiKey").And.Contain("Required for auth");
        ex.OptionName.Should().Be("ApiKey");
    }

    [Fact]
    public void VKDependencyException_DualRegistrationMissing_ShouldSetOptionsType()
    {
        var ex = VKDependencyException.DualRegistrationMissing("MyOptions");
        ex.Message.Should().Contain("MyOptions");
    }
}
