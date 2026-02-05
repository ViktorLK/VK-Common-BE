using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using VK_Common_BE.Constants;
using VK_Common_BE.Data;
using VK_Common_BE.Repositories;
using VK_Common_BE.Services;

namespace VK_Common_BE.Extensions
{
    /// <summary>
    /// サービス登録拡張メソッド
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// アプリケーションサービスを追加
        /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();

            // リポジトリ層を登録
            services.AddScoped<IProductRepository, ProductRepository>();

            // Azure Blob Storage サービスを登録
            services.AddAzureBlobStorageServices(configuration);

            // サービス層を登録
            services.AddScoped<IProductService, ProductService>();

            // GraphQLサービスを登録
            services.AddGraphQLServices();

            // HttpContextAccessorを登録（Authorization Handlerで使用）
            services.AddHttpContextAccessor();

            return services;
        }

        /// <summary>
        /// Azure Blob Storage サービスを追加
        /// </summary>
        public static IServiceCollection AddAzureBlobStorageServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // BlobServiceClientをシングルトンとして登録（DefaultAzureCredential使用）
            services.AddSingleton(sp =>
            {
                var storageAccountName = configuration["AzureStorage:AccountName"]
                    ?? throw new InvalidOperationException("AzureStorage:AccountName configuration is required");

                var blobServiceUri = new Uri($"https://{storageAccountName}.blob.core.windows.net");
                return new BlobServiceClient(blobServiceUri, new DefaultAzureCredential());
            });

            // Azure Blob Storage リポジトリを登録
            services.AddScoped<IAzureBlobStorageRepository, AzureBlobStorageRepository>();

            return services;
        }

        /// <summary>
        /// データベースサービスを追加
        /// </summary>
        public static IServiceCollection AddDatabaseServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(configuration.GetConnectionString(ConfigurationKeys.ConnectionStrings.Default)));

            return services;
        }

        /// <summary>
        /// Swaggerドキュメントサービスを追加
        /// </summary>
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(RouteConstants.Swagger.Version, new OpenApiInfo
                {
                    Title = RouteConstants.Swagger.Title,
                    Version = RouteConstants.Swagger.Version,
                    Description = RouteConstants.Swagger.Description
                });
            });

            return services;
        }

        /// <summary>
        /// CORSポリシーを追加
        /// </summary>
        public static IServiceCollection AddCorsPolicy(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // 設定から許可するオリジンを取得
            var allowedOrigins = configuration.GetSection(ConfigurationKeys.Cors.SectionName)
                .Get<string[]>() ?? Array.Empty<string>();

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    // 許可するオリジンが設定されている場合
                    if (allowedOrigins.Length > 0)
                    {
                        policy.WithOrigins(allowedOrigins)
                              .AllowAnyMethod()
                              .AllowAnyHeader()
                              .AllowCredentials();  // 認証情報を許可
                    }
                    else
                    {
                        // 設定がない場合は開発用に全許可（本番環境では設定必須）
                        policy.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    }
                });
            });

            return services;
        }

        /// <summary>
        /// 二重認証サービスを追加（Azure B2C + APIキー）
        /// </summary>
        public static IServiceCollection AddAuthenticationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Azure B2C 設定の検証
            var azureB2CSection = configuration.GetSection(ConfigurationKeys.AzureB2C.SectionName);
            var instance = azureB2CSection["Instance"];
            var clientId = azureB2CSection["ClientId"];

            // 有効な Azure B2C 設定があるかチェック（プレースホルダーでない実際の値）
            bool hasValidAzureB2CConfig =
                !string.IsNullOrEmpty(instance) &&
                !string.IsNullOrEmpty(clientId) &&
                !instance.Contains("yourtenantname") &&
                !clientId.Contains("your-client-id");

            // 認証スキームを設定
            var authBuilder = services.AddAuthentication(options =>
            {
                // Azure B2C が有効な場合は動的スキーム、無効な場合は API キーのみ
                if (hasValidAzureB2CConfig)
                {
                    options.DefaultAuthenticateScheme = AuthenticationConstants.Schemes.Dynamic;
                    options.DefaultChallengeScheme = AuthenticationConstants.Schemes.Dynamic;
                }
                else
                {
                    options.DefaultAuthenticateScheme = AuthenticationConstants.Schemes.ApiKey;
                    options.DefaultChallengeScheme = AuthenticationConstants.Schemes.ApiKey;
                }
            });

            // 1. APIキー認証（常に有効）
            authBuilder.AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, VK_Common_BE.Authentication.ApiKeyAuthenticationHandler>(
                AuthenticationConstants.Schemes.ApiKey,
                options => { });

            // 2. Azure B2C認証（設定が有効な場合のみ）
            if (hasValidAzureB2CConfig)
            {
                authBuilder.AddPolicyScheme(AuthenticationConstants.Schemes.Dynamic, "Dynamic Authentication Scheme", options =>
                {
                    // リクエストヘッダーにAPIKeyがあるかどうかで認証スキームを選択
                    options.ForwardDefaultSelector = context =>
                    {
                        if (context.Request.Headers.ContainsKey(AuthenticationConstants.ApiKeyHeaderName))
                        {
                            return AuthenticationConstants.Schemes.ApiKey;
                        }
                        return AuthenticationConstants.Schemes.AzureB2C;
                    };
                })
                // Microsoft.Identity.Web使用
                .AddMicrosoftIdentityWebApi(
                    azureB2CSection,
                    AuthenticationConstants.Schemes.AzureB2C
                );
            }

            // カスタム認可ハンドラーを登録
            services.AddSingleton<IAuthorizationHandler, VK_Common_BE.Authorization.ApiKeyReadOnlyHandler>();

            // 認可ポリシーを追加
            services.AddAuthorization(options =>
            {
                // ロールベースのポリシー
                options.AddPolicy(AuthenticationConstants.Policies.RequireAdminRole, policy =>
                    policy.RequireRole(AuthenticationConstants.Roles.Admin));

                options.AddPolicy(AuthenticationConstants.Policies.RequireUserRole, policy =>
                    policy.RequireRole(AuthenticationConstants.Roles.User, AuthenticationConstants.Roles.Admin));

                // 組み合わせポリシー：いずれかの認証スキームを許可 + APIキーはGETのみ
                options.AddPolicy(AuthenticationConstants.Policies.ApiOrB2C, policy =>
                {
                    policy.AddAuthenticationSchemes(AuthenticationConstants.Schemes.ApiKey, AuthenticationConstants.Schemes.AzureB2C)
                          .RequireAuthenticatedUser()
                          .AddRequirements(new VK_Common_BE.Authorization.ApiKeyReadOnlyRequirement());
                });
            });

            return services;
        }

        /// <summary>
        /// モニタリングサービス（Application Insights, Health Checks）を追加
        /// </summary>
        public static IServiceCollection AddMonitoringServices(this IServiceCollection services)
        {
            // Application Insights の登録
            services.AddApplicationInsightsTelemetry();

            // ヘルスチェックの登録
            services.AddHealthChecks()
                // データベース接続確認 (ApplicationDbContext)
                .AddDbContextCheck<ApplicationDbContext>();

            return services;
        }
    }
}
