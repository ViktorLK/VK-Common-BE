using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience.Http.Internal;

// [AP.01] sealed
internal sealed class VKResilienceDelegatingHandler : DelegatingHandler
{
    private readonly string _pipelineName;
    private readonly IServiceProvider _serviceProvider;

    public VKResilienceDelegatingHandler(string pipelineName, IServiceProvider serviceProvider)
    {
        _pipelineName = VKGuard.NotNullOrWhiteSpace(pipelineName);
        _serviceProvider = VKGuard.NotNull(serviceProvider);
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var registry = _serviceProvider.GetRequiredService<IVKPolicyRegistry>();
        var pipeline = registry.GetPipeline(_pipelineName);

        var context = VKResilienceContext.Create(
            operationKey: $"http:{request.Method}:{request.RequestUri?.Host ?? "unknown"}",
            operationName: "HttpClient.SendAsync");

        var result = await pipeline.ExecuteAsync<HttpResponseMessage>(
            async (_, ct) =>
            {
                var response = await base.SendAsync(request, ct).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode && (int)response.StatusCode >= 500)
                {
                    return VKResult.Failure<HttpResponseMessage>(
                        new VKError($"Http.{(int)response.StatusCode}", $"HTTP request failed with status code {response.StatusCode}"));
                }
                return VKResult.Success(response);
            },
            context: context,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        if (result.IsSuccess)
        {
            return result.Value;
        }

        // If resilience attempts were exhausted with an error, throw an HttpRequestException
        throw new HttpRequestException($"Resilience pipeline '{_pipelineName}' failed: {result.FirstError.Description}");
    }
}
