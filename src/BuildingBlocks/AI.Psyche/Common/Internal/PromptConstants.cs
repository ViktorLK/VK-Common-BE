namespace VK.Blocks.AI.Psyche.Common.Internal;

// // [AP.03] Internal constants in internal namespace
internal static class PsycheConstants
{
    internal static class XmlTags
    {
        internal const string SystemDirectives = "system_directives";
        internal const string Message = "message";
        internal const string Knowledge = "knowledge";
        internal const string ImportantKnowledge = "important_knowledge";
        internal const string Persona = "persona";
    }

    internal static class ChatML
    {
        internal const string ImStart = "<|im_start|>";
        internal const string ImEnd = "<|im_end|>";
    }

    internal static class Separators
    {
        internal const string DefaultSegment = "\n\n";
        internal const string DefaultRoleHeader = ": ";
    }

    internal static class Layout
    {
        internal const int RelativeOffset = 1000;
        internal const int EchoReserve = 10000;
        internal const int TierCoordinateGap = 10000;
    }
}
