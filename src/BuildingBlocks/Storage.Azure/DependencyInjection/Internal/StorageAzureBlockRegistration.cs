using System;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.Infrastructure.Azure;
using VK.Blocks.Infrastructure.Azure.Abstractions;
using VK.Blocks.Storage.Azure.Internal.Providers;
using VK.Blocks.Storage.Azure.Internal.Services;

namespace VK.Blocks.Storage.Azure.DependencyInjection.Internal;

/// <summary>
/// Principal registration logic for the Storage Azure block.
/// </summary>
internal static class StorageAzureBlockRegistration
{
    public static IVKStorageAzureBuilder Register(IServiceCollection services, IConfiguration configuration)
    {
        // 1. Check-Self & Prerequisite
        if (services.IsVKBlockRegistered<VKStorageAzureBlock>())
        {
            return new StorageAzureBlockBuilder(services);
        }

        // 2. Options Registration
        var options = services.AddVKBlockOptions<VKStorageOptions>(configuration);

        // 3. Mark-Self
        services.AddVKBlockMarker<VKStorageAzureBlock>();

        // 4. Options Validation
        services.TryAddEnumerableSingleton<IValidateOptions<VKStorageOptions>, VKStorageOptionsValidator>();

        // 5. Diagnostics
        // services.AddVKBlockDiagnostics<VKStorageAzureBlock>();

        // 6. Feature Toggle
        if (!options.Enabled)
        {
            return new StorageAzureBlockBuilder(services);
        }

        // 7. Core Services
        services.TryAddScoped<IVKStorageContainerProvider, DefaultStorageContainerProvider>();

        // Hybrid registration for BlobServiceClient
        services.TryAddSingleton(sp =>
        {
            var storageOptions = sp.GetRequiredService<IOptions<VKStorageOptions>>().Value;
            var azureOptions = sp.GetRequiredService<IOptions<VKAzureSharedOptions>>().Value;

            // 1. Connection String Priority (Local > Shared)
            var connectionString = !string.IsNullOrWhiteSpace(storageOptions.ConnectionString)
                ? storageOptions.ConnectionString
                : azureOptions.ConnectionString;

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                return new BlobServiceClient(connectionString);
            }

            // 2. Identity Priority (Requires ServiceUri)
            if (!string.IsNullOrWhiteSpace(storageOptions.ServiceUri))
            {
                var credentialProvider = sp.GetRequiredService<IAzureCredentialProvider>();
                return new BlobServiceClient(new Uri(storageOptions.ServiceUri), credentialProvider.GetCredential());
            }

            throw new InvalidOperationException(
                "Failed to initialize BlobServiceClient. Provide 'ConnectionString' (Storage or Infrastructure) or 'ServiceUri' for Identity-based access.");
        });

        // Base Services
        services.TryAddScoped<StorageFileService>();
        services.TryAddScoped<IVKStorageFileService>(sp => sp.GetRequiredService<StorageFileService>());
        services.TryAddScoped<IVKStorageUriGenerator>(sp => sp.GetRequiredService<StorageFileService>());

        // Advanced Services
        services.TryAddScoped<IVKStorageContainerService, StorageContainerService>();
        services.TryAddScoped<IVKStorageLeaseService, StorageLeaseService>();
        services.TryAddScoped<IVKStorageTagService, StorageTagService>();
        services.TryAddScoped<IVKStorageDirectoryService, StorageDirectoryService>();

        // Facade
        services.TryAddScoped<IVKStorageService, StorageService>();

        return new StorageAzureBlockBuilder(services);
    }
}
