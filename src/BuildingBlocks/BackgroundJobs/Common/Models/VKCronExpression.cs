using System;
using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs;

/// <summary>
/// Wrapper for Cron schedule expressions.
/// </summary>
public sealed record VKCronExpression
{
    public string Expression { get; }

    public VKCronExpression(string expression)
    {
        Expression = VKGuard.NotNullOrWhiteSpace(expression);
    }

    public static VKCronExpression Minutely => new("* * * * *");
    public static VKCronExpression Hourly => new("0 * * * *");
    public static VKCronExpression Daily => new("0 0 * * *");
    public static VKCronExpression Weekly => new("0 0 * * 0");
    public static VKCronExpression Monthly => new("0 0 1 * *");

    public DateTimeOffset? GetNextOccurrence(DateTimeOffset fromTime)
    {
        return fromTime.AddMinutes(1);
    }
}
