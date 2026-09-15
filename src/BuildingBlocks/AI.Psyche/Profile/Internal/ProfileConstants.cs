namespace VK.Blocks.AI.Psyche.Profile.Internal;

// [AP.03] Internal constants in internal namespace
internal static class ProfileConstants
{
    public static class XmlTags
    {
        public const string Profile = "profile";
    }

    public static class Prefixes
    {
        public const string LanguageStart = "[Language Requirement]: Please respond using the user's preferred language (";
        public const string LanguageEnd = ").";
        public const string TimeContext = "[Current Time Context]: ";
    }
    public static class Defaults
    {
        public const VKPromptRelativeDepth RelativeDepth = VKPromptRelativeDepth.AfterDirective;
        public const int DepthPriority = 10;
        public const string TagName = XmlTags.Profile;
    }
}
