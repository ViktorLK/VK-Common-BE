using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VK.Labs.PersonaWeavePulsar.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// 1. Add PWP Backend Services
builder.Services.AddPwpServices(builder.Configuration);

// 2. Add Controller Support
builder.Services.AddControllers();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<VK.Labs.PersonaWeavePulsar.Web.OpenApi.TenantHeaderOperationFilter>();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
