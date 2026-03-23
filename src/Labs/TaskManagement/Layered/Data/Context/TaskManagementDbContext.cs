using Microsoft.EntityFrameworkCore;
using VK.Labs.TaskManagement.Layered.Data.Entities;
using VK.Blocks.MultiTenancy.Abstractions;

namespace VK.Labs.TaskManagement.Layered.Data.Context;

public class TaskManagementDbContext : DbContext
{
    private readonly string? _currentTenantId;

    public TaskManagementDbContext(DbContextOptions<TaskManagementDbContext> options) 
        : base(options)
    {
    }

    // Constructor to support multi-tenancy manually for now (in real VK.Blocks, use ICurrentTenant)
    public TaskManagementDbContext(DbContextOptions<TaskManagementDbContext> options, ITenantProvider tenantProvider) 
        : base(options)
    {
        _currentTenantId = tenantProvider.GetCurrentTenantId();
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global Query Filters for Multi-Tenancy
        modelBuilder.Entity<User>().HasQueryFilter(e => _currentTenantId == null || e.TenantId == _currentTenantId);
        modelBuilder.Entity<Project>().HasQueryFilter(e => _currentTenantId == null || e.TenantId == _currentTenantId);
        modelBuilder.Entity<ProjectMember>().HasQueryFilter(e => _currentTenantId == null || e.TenantId == _currentTenantId);
        modelBuilder.Entity<TaskItem>().HasQueryFilter(e => _currentTenantId == null || e.TenantId == _currentTenantId);

        // Configure relations
        modelBuilder.Entity<ProjectMember>()
            .HasOne(pm => pm.Project)
            .WithMany(p => p.Members)
            .HasForeignKey(pm => pm.ProjectId);

        modelBuilder.Entity<ProjectMember>()
            .HasOne(pm => pm.User)
            .WithMany(u => u.ProjectMemberships)
            .HasForeignKey(pm => pm.UserId);

        modelBuilder.Entity<TaskItem>()
            .HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId);
    }
}
