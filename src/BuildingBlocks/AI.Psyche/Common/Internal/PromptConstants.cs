namespace VK.Blocks.AI.Psyche.Common.Internal;

// // [AP.03] Internal constants in internal namespace
internal static class PsycheConstants
{
    internal static class XmlTags
    {
        internal const string SystemDirective = "system_directive";
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
        internal const string SegmentSeparator = "\n\n";
        internal const string DefaultRoleHeader = ": ";
    }

    internal static class LayoutSlots
    {
        // Pillar 1: Directive
        internal const int BeforeDirective = 100_000;
        internal const int Directive = 200_000;
        internal const int AfterDirective = 300_000;

        // Pillar 2: Persona
        internal const int BeforePersona = 400_000;
        internal const int Persona = 500_000;
        internal const int AfterPersona = 600_000;

        // Static boundary
        internal const int StaticBoundary = 700_000;
    }
}
