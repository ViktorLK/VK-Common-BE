namespace VK.Blocks.AI.Psyche.Weaving.Internal;

// // [AP.03] Internal constants in internal namespace
internal static class PromptConstants
{
    public static class XmlTags
    {
        public const string SystemDirectives = "system_directives";
        public const string Message = "message";
    }

    public static class ChatML
    {
        public const string ImStart = "<|im_start|>";
        public const string ImEnd = "<|im_end|>";
    }

    public static class Separators
    {
        public const string DefaultSegment = "\n\n";
        public const string DefaultRoleHeader = ": ";
    }
}
