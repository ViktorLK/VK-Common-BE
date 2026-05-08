using VK.Blocks.Core;

namespace VK.Blocks.Storage;

public sealed class VKStorageValidationException(
    string message,
    string code) : VKBaseException(code, message, 400, true);
