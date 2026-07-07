using System;
using System.Collections.Generic;

namespace VK.Blocks.AI.Afferent;

public record VKSystemEvent(
    DateTimeOffset Timestamp,
    string Category,
    string EventName,
    IReadOnlyDictionary<string, object> Payload
);
