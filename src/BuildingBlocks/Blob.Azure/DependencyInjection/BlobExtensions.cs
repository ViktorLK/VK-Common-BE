using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Blob.Abstractions;
using VK.Blocks.Blob.Options;
using VK.Blocks.Blob.Providers;
using VK.Blocks.Blob.Services;

namespace VK.Blocks.Blob.DependencyInjection;

public static class BlobExtensions
{
    public static IServiceCollection AddBlobStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var optionsSection = configuration.GetSection(BlobOptions.SectionName);
        services.Configure<BlobOptions>(optionsSection);
        services.AddSingleton<IValidateOptions<BlobOptions>, BlobOptionsValidator>();

        var blobOptions = optionsSection.Get<BlobOptions>() ?? new BlobOptions();

        services.TryAddScoped<IBlobContainerProvider, DefaultBlobContainerProvider>();

        var clientOptions = new BlobClientOptions
        {
            Retry =
            {
                Delay = TimeSpan.FromSeconds(2),
                MaxRetries = 3,
                Mode = RetryMode.Exponential
            }
        };

        // Hybrid registration for BlobServiceClient
        services.AddSingleton(sp =>
        {
            if (!string.IsNullOrWhiteSpace(blobOptions.ConnectionString))
            {
                return new BlobServiceClient(blobOptions.ConnectionString, clientOptions);
            }

            if (!string.IsNullOrWhiteSpace(blobOptions.ServiceUri))
            {
                return new BlobServiceClient(new Uri(blobOptions.ServiceUri), new DefaultAzureCredential(), clientOptions);
            }

            throw new InvalidOperationException("Either BlobStorage:ConnectionString or BlobStorage:ServiceUri must be provided.");
        });

        // Base Services
        services.AddScoped<BlobFileService>();
        services.AddScoped<IBlobFileService>(sp => sp.GetRequiredService<BlobFileService>());
        services.AddScoped<IBlobUriGenerator>(sp => sp.GetRequiredService<BlobFileService>());

        // Advanced Services
        services.AddScoped<IBlobContainerService, BlobContainerService>();
        services.AddScoped<IBlobLeaseService, BlobLeaseService>();
        services.AddScoped<IBlobTagService, BlobTagService>();
        services.AddScoped<IBlobDirectoryService, BlobDirectoryService>();

        // Facade
        services.AddScoped<IBlobService, BlobService>();

        return services;
    }
}
