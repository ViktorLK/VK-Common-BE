namespace VK.Blocks.Validation;

/// <summary>
/// Constants for pagination validation.
/// Following AP.03: Flat structure for public constants.
/// </summary>
public static class VKPaginationConstants
{
    // Performance Guards
    public const int MaxOffsetLimit = 10000;
    public const int MaxCursorLimit = 10000;
    public const int MaxPageSize = 1000;

    // Error Messages
    public const string PageNumberMustBePositive = "Page number must be positive.";
    public const string PageSizeMustBePositive = "Page size must be positive.";
    public const string OverOffsetLimit = "Offset limit exceeded.";
    public const string OverPageSize = "Page size exceeded.";
}

