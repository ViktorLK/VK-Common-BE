using VK.Blocks.Core.Exceptions;

namespace VK.Blocks.Blob.Exceptions;

public sealed class BlobValidationException(
    string message,
    string code) : BaseException(code, message, 400, true);
