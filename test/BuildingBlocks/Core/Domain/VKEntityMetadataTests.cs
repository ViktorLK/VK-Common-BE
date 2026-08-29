namespace VK.Blocks.Core.UnitTests.Domain;

public class VKEntityMetadataTests
{
    private sealed class PlainEntity;

    private sealed class AuditableEntity : IVKAuditable
    {
        public DateTimeOffset CreatedAt { get; set; }
        public VKUserId? CreatedBy { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public VKUserId? UpdatedBy { get; set; }
    }

    private sealed class SoftDeleteEntity : IVKSoftDeletable
    {
        public bool IsDeleted { get; set; }
    }

    private sealed class MultiTenantEntityStub : IVKTenantScoped
    {
        public VKTenantId TenantId { get; set; }
    }

    private sealed class DeletionAuditedEntity : IVKDeletionAudited
    {
        public DateTimeOffset? DeletedAt { get; set; }
        public VKUserId? DeletedBy { get; set; }
    }

    private sealed class AllInOneEntity : IVKAuditable, IVKSoftDeletable, IVKDeletionAudited, IVKTenantScoped
    {
        public DateTimeOffset CreatedAt { get; set; }
        public VKUserId? CreatedBy { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public VKUserId? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public VKUserId? DeletedBy { get; set; }
        public VKTenantId TenantId { get; set; }
    }

    [Fact]
    public void IsAuditable_WhenImplementsIAuditable_ReturnsTrue()
    {
        VKEntityMetadata.IsAuditable(typeof(AuditableEntity)).Should().BeTrue();
        VKEntityMetadata.IsAuditable(typeof(AllInOneEntity)).Should().BeTrue();
        VKEntityMetadata.IsAuditable(typeof(PlainEntity)).Should().BeFalse();
        VKEntityMetadata.IsAuditable(typeof(SoftDeleteEntity)).Should().BeFalse();
    }

    [Fact]
    public void IsSoftDelete_WhenImplementsISoftDeletable_ReturnsTrue()
    {
        VKEntityMetadata.IsSoftDelete(typeof(SoftDeleteEntity)).Should().BeTrue();
        VKEntityMetadata.IsSoftDelete(typeof(AllInOneEntity)).Should().BeTrue();
        VKEntityMetadata.IsSoftDelete(typeof(PlainEntity)).Should().BeFalse();
        VKEntityMetadata.IsSoftDelete(typeof(AuditableEntity)).Should().BeFalse();
    }

    [Fact]
    public void IsDeletionAudited_WhenImplementsIDeletionAudited_ReturnsTrue()
    {
        VKEntityMetadata.IsDeletionAudited(typeof(DeletionAuditedEntity)).Should().BeTrue();
        VKEntityMetadata.IsDeletionAudited(typeof(AllInOneEntity)).Should().BeTrue();
        VKEntityMetadata.IsDeletionAudited(typeof(PlainEntity)).Should().BeFalse();
        VKEntityMetadata.IsDeletionAudited(typeof(AuditableEntity)).Should().BeFalse();
    }

    [Fact]
    public void IsMultiTenant_WhenImplementsIVKTenantScoped_ReturnsTrue()
    {
        VKEntityMetadata.IsMultiTenant(typeof(MultiTenantEntityStub)).Should().BeTrue();
        VKEntityMetadata.IsMultiTenant(typeof(AllInOneEntity)).Should().BeTrue();
        VKEntityMetadata.IsMultiTenant(typeof(PlainEntity)).Should().BeFalse();
    }

    [Fact]
    public void IsAssignableTo_ReturnsCorrectResult()
    {
        VKEntityMetadata.IsAssignableTo(typeof(AuditableEntity), typeof(IVKAuditable)).Should().BeTrue();
        VKEntityMetadata.IsAssignableTo(typeof(PlainEntity), typeof(IVKAuditable)).Should().BeFalse();
    }
}
