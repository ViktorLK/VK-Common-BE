using System.Diagnostics;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VK.Blocks.BackgroundJobs.Abstractions;
using VK.Blocks.BackgroundJobs.Abstractions.Contracts;
using VK.Blocks.BackgroundJobs.Hangfire.Diagnostics;
using VK.Blocks.MultiTenancy.Abstractions;
using VK.Blocks.MultiTenancy.Abstractions.Contracts;
using VK.Blocks.MultiTenancy.Context;
using VK.Blocks.Observability.Conventions;
using VK.Blocks.Observability.Extensions;

namespace VK.Blocks.BackgroundJobs.Hangfire.Services;

public sealed class HangfireJobService(
    IBackgroundJobClient backgroundJobClient,
    ITenantProvider tenantProvider) : IBackgroundJobService
{
    public string Enqueue<TJob, TData>(TData data) where TJob : IJobHandler<TData>
    {
        // Capture context for the background job
        var tenantId = tenantProvider.GetCurrentTenantId();
        var correlationId = Activity.Current?.Id;
        
        return backgroundJobClient.Enqueue<HangfireJobRunner>(
            runner => runner.RunAsync<TJob, TData>(data, tenantId, correlationId, CancellationToken.None));
    }
}

/// <summary>
/// Internal bridge to resolve the job handler from DI and execute it in Hangfire.
/// </summary>
internal sealed class HangfireJobRunner(
    IServiceProvider serviceProvider,
    ILogger<HangfireJobRunner> logger)
{
    [JobDisplayName("{0}")]
    public async Task RunAsync<TJob, TData>(TData data, string? tenantId, string? correlationId, CancellationToken ct) 
        where TJob : IJobHandler<TData>
    {
        var jobType = typeof(TJob).Name;
        
        using var activity = BackgroundJobDiagnostics.Source.StartActivity(
            $"Job: {jobType}", 
            ActivityKind.Consumer, 
            correlationId);

        activity?.SetTag(FieldNames.CorrelationId, correlationId);
        activity.SetTenantId(tenantId);
        activity?.SetTag("job.type", jobType);

        using var scope = serviceProvider.CreateScope();
        logger.LogInformation("Starting background job {JobType} for tenant {TenantId}. CorrelationId: {CorrelationId}", 
            jobType, tenantId, correlationId);

        try
        {
            // Set tenant context for the background execution if provided
            if (!string.IsNullOrEmpty(tenantId))
            {
                var tenantContext = scope.ServiceProvider.GetRequiredService<TenantContext>();
                tenantContext.SetTenant(new TenantInfo(tenantId, "Background Job Tenant"));
            }

            var handler = scope.ServiceProvider.GetRequiredService<TJob>();
            
            var context = new JobContext(
                JobId: Guid.NewGuid().ToString(), 
                TenantId: tenantId,
                CorrelationId: correlationId);

            var result = await handler.ExecuteAsync(data, context, ct);
            
            if (result == JobExecutionResult.Failed)
            {
                logger.LogWarning("Job {JobType} completed with Failure state.", jobType);
                activity?.SetStatus(ActivityStatusCode.Error, "Job completed with Failure result");
            }
            else
            {
                logger.LogInformation("Job {JobType} completed successfully.", jobType);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error executing job {JobType} for tenant {TenantId}.", jobType, tenantId);
            
            // Standard OTel exception recording
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddEvent(new ActivityEvent("exception", tags: new ActivityTagsCollection { { "exception.message", ex.Message }, { "exception.stacktrace", ex.StackTrace } }));
            
            throw; // Let Hangfire handle retries
        }
    }
}
