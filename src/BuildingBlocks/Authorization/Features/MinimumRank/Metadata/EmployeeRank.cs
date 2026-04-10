

namespace VK.Blocks.Authorization.Features.MinimumRank.Metadata;

/// <summary>
/// Rank hierarchy understood by <c>MinimumRankAuthorizationHandler</c> (ascending order).
/// </summary>
[GenerateRankAuthorize]
public enum EmployeeRank
{
    Junior = 0,
    Mid = 1,
    Senior = 2,
    Lead = 3,
    Principal = 4
}
