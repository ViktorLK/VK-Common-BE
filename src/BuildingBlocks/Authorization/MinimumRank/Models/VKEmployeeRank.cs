namespace VK.Blocks.Authorization;

/// <summary>
/// Rank hierarchy understood by <c>VKMinimumRankAuthorizationHandler</c> (ascending order).
/// </summary>
[VKGenerateRankAuthorize]
public enum VKEmployeeRank
{
    Junior = 0,
    Mid = 1,
    Senior = 2,
    Lead = 3,
    Principal = 4
}
