using System;

namespace VK.Blocks.AI.Cognitive;

public sealed record VKPipelineError
{
    public required string StageName { get; init; }
    public required string Message { get; init; }
    public Exception? Exception { get; init; }

    public static VKPipelineError From<TStage>(string message, Exception? ex = null)
        => new()
        {
            StageName = typeof(TStage).Name,
            Message = message,
            Exception = ex
        };
}
