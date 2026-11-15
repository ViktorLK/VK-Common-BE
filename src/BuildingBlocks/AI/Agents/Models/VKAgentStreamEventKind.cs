namespace VK.Blocks.AI;

/// <summary>
/// Categorizes streaming events emitted during a single-agent run.
/// </summary>
public enum VKAgentStreamEventKind : byte
{
    /// <summary>
    /// A new reasoning/LLM step started.
    /// </summary>
    StepStarted = 0,

    /// <summary>
    /// An incremental text token delta produced by the model.
    /// </summary>
    TokenDelta = 1,

    /// <summary>
    /// An atomic tool call started executing.
    /// </summary>
    ToolCallStarted = 2,

    /// <summary>
    /// An atomic tool call completed executing.
    /// </summary>
    ToolCallCompleted = 3,

    /// <summary>
    /// A reasoning/LLM step finished.
    /// </summary>
    StepCompleted = 4,

    /// <summary>
    /// The overall agent run completed.
    /// </summary>
    Completed = 5
}
