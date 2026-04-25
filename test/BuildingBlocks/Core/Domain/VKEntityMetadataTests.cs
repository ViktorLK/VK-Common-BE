namespace VK.Blocks.Core.UnitTests.Domain;

public class VKEntityMetadataTests
{
    private sealed class PlainEntity;

    private sealed class AuditableEntity : IVKAuditable
    {
        public DateTimeOffset CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }

    private sealed class SoftDeleteEntity : IVKSoftDelete
    {
        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }

    private sealed class MultiTenantEntityStub : IVKMultiTenant
    {
        public string? TenantId { get; set; }
    }

    private sealed class FullMultiTenantEntity : IVKMultiTenantEntity
    {
        public string? TenantId { get; set; }
    }

    private sealed class AllInOneEntity : IVKAuditable, IVKSoftDelete, IVKMultiTenantEntity
    {
        public DateTimeOffset CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        public string? TenantId { get; set; }
    }

    [Fact]
    public void IsAuditable_WhenImplementsIAuditable_ReturnsTrue()
    {
        VKEntityMetadata.IsAuditable(typeof(AuditableEntity)).Should().BeTrue();
        VKEntityMetadata.IsAuditable(typeof(AllInOneEntity)).Should().BeTrue();
    }

    [Fact]
    public void IsAuditable_WhenNotImplementsIAuditable_ReturnsFalse()
    {
        VKEntityMetadata.IsAuditable(typeof(PlainEntity)).Should().BeFalse();
        VKEntityMetadata.IsAuditable(typeof(SoftDeleteEntity)).Should().BeFalse();
    }

    [Fact]
    public void IsSoftDelete_WhenImplementsISoftDelete_ReturnsTrue()
    {
        VKEntityMetadata.IsSoftDelete(typeof(SoftDeleteEntity)).Should().BeTrue();
        VKEntityMetadata.IsSoftDelete(typeof(AllInOneEntity)).Should().BeTrue();
    }

    [Fact]
    public void IsSoftDelete_WhenNotImplementsISoftDelete_ReturnsFalse()
    {
        VKEntityMetadata.IsSoftDelete(typeof(PlainEntity)).Should().BeFalse();
        VKEntityMetadata.IsSoftDelete(typeof(AuditableEntity)).Should().BeFalse();
    }

    [Fact]
    public void IsMultiTenant_WhenImplementsIMultiTenant_ReturnsTrue()
    {
        VKEntityMetadata.IsMultiTenant(typeof(MultiTenantEntityStub)).Should().BeTrue();
        VKEntityMetadata.IsMultiTenant(typeof(FullMultiTenantEntity)).Should().BeTrue();
        VKEntityMetadata.IsMultiTenant(typeof(AllInOneEntity)).Should().BeTrue();
    }

    [Fact]
    public void IsMultiTenantEntity_WhenImplementsIMultiTenantEntity_ReturnsTrue()
    {
        VKEntityMetadata.IsMultiTenantEntity(typeof(FullMultiTenantEntity)).Should().BeTrue();
        VKEntityMetadata.IsMultiTenantEntity(typeof(AllInOneEntity)).Should().BeTrue();
    }

    [Fact]
    public void IsMultiTenantEntity_WhenOnlyImplementsIMultiTenant_ReturnsFalse()
    {
        VKEntityMetadata.IsMultiTenantEntity(typeof(MultiTenantEntityStub)).Should().BeFalse();
    }

    [Fact]
    public void IsAssignableTo_ReturnsCorrectResult()
    {
        VKEntityMetadata.IsAssignableTo(typeof(AuditableEntity), typeof(IVKAuditable)).Should().BeTrue();
        VKEntityMetadata.IsAssignableTo(typeof(PlainEntity), typeof(IVKAuditable)).Should().BeFalse();
    }
}
