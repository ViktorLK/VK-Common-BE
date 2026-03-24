using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VK.Blocks.Authentication.DependencyInjection;
using VK.Blocks.Authorization.Abstractions;
using VK.Blocks.Authorization.DependencyInjection;
using VK.Blocks.ExceptionHandling.DependencyInjection;
using VK.Blocks.MultiTenancy.DependencyInjection;
using VK.Blocks.Validation.DependencyInjection;
using VK.Labs.TaskManagement.Layered.Data.Context;
using VK.Labs.TaskManagement.Layered.Data.Repositories.Implementations;
using VK.Labs.TaskManagement.Layered.Data.Repositories.Interfaces;
using VK.Labs.TaskManagement.Layered.Services.Implementations;
using VK.Labs.TaskManagement.Layered.Services.Interfaces;
using VK.Labs.TaskManagement.Layered.Services.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options => 
{
    options.Filters.Add<VK.Blocks.Validation.Filters.ValidationActionFilter>();
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure EF Core with In-Memory DB for this Lab instead of SQL Server to keep it fully functional out of the box
builder.Services.AddDbContext<TaskManagementDbContext>(options =>
    options.UseInMemoryDatabase("TaskManagementLabDb"));

// Authentication and Authorization Building Blocks
builder.Services.AddDistributedMemoryCache(); // Required by Authentication BB TokenBlacklist
builder.Services.AddVKAuthenticationBlock(builder.Configuration);
builder.Services.AddVKAuthorization();
builder.Services.AddAuthorizationBuilder()
    .AddVKPolicies();

// Exception Handling, MultiTenancy, and Validation Building Blocks
builder.Services.AddExceptionHandling(options =>
{
    options.ExposeStackTrace = builder.Environment.IsDevelopment();
});
builder.Services.AddMultiTenancy(
    options => 
    {
        options.EnforceTenancy = false;
        options.EnabledResolvers = [
            VK.Blocks.MultiTenancy.Options.TenantResolverType.Claims,
            VK.Blocks.MultiTenancy.Options.TenantResolverType.Header
        ];
    },
    resolution => resolution.ClaimType = VK.Blocks.Authentication.Claims.VKClaimTypes.TenantId
);
builder.Services.AddVKValidation(options =>
{
    options.EnableFluentValidation = true;
    options.EnableDataAnnotations = false;
});

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();

// Services
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPermissionProvider, TaskManagementPermissionProvider>();

// Validators
builder.Services.AddValidatorsFromAssemblyContaining<CreateProjectValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseVKExceptionHandling(); // Global exception handling first

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use Authentication before our custom middleware which reads resulting Claims
app.UseAuthentication();

// Use MultiTenancy BB Tenant Resolution
app.UseMultiTenancy();

app.UseAuthorization();

app.MapControllers();

app.Run();
