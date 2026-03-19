namespace VK.Blocks.ExceptionHandling.Constants;

/// <summary>
/// Constants used throughout the ExceptionHandling module.
/// </summary>
public static class ExceptionHandlingConstants
{
    /// <summary>
    /// Standard error codes used in ProblemDetails.
    /// </summary>
    public static class ErrorCodes
    {
        public const string NotFound = "NotFound";
        public const string Unauthorized = "Unauthorized";
        public const string ValidationErrors = "ValidationErrors";
        public const string InternalServerError = "InternalServerError";
    }

    /// <summary>
    /// Standard titles for ProblemDetails responses.
    /// </summary>
    public static class ProblemDetailsTitles
    {
        public const string BadRequest = "Bad Request";
        public const string Unauthorized = "Unauthorized";
        public const string Forbidden = "Forbidden";
        public const string NotFound = "Not Found";
        public const string InternalServerError = "Internal Server Error";
    }

    /// <summary>
    /// Keys used in ProblemDetails.Extensions dictionary.
    /// </summary>
    public static class ExtensionKeys
    {
        public const string StackTrace = "stackTrace";
        public const string Errors = "errors";
    }
}
