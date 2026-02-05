using VK_Common_BE.Extensions;

var builder = WebApplication.CreateBuilder(args);

// サービスを登録
builder.Services
    .AddApplicationServices(builder.Configuration)
    .AddDatabaseServices(builder.Configuration)
    .AddAuthenticationServices(builder.Configuration)  // 認証サービス
    .AddSwaggerDocumentation()
    .AddCorsPolicy(builder.Configuration)  // CORS ポリシー
    .AddMonitoringServices(); // モニタリング (App Insights, Health Checks)

var app = builder.Build();

// ミドルウェアを設定
app.ConfigureMiddleware();

app.Run();
