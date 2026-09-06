namespace VK.Blocks.AI.Eidos.Negotiation.Internal;

internal static class NegotiationConstants
{
    public const string ResponseFormatHeader = "[Response Format Requirement]";
    public const string ExampleFormatHeader = "[Example Valid Response Format]";
    public const string StrictJsonDirectivePrefix = "Respond strictly using JSON matching schema: ";
    public const string StrictJsonProtocolHeader = "[EIDOS_STRICT_JSON_PROTOCOL]";
    public const string StrictJsonProtocolContent = "[EIDOS_STRICT_JSON_PROTOCOL]\nYou are operating in STRICT JSON execution mode. Your sole response medium is raw, parseable JSON. Do NOT include markdown code blocks, conversational filler, greetings, or explanations.";
}
