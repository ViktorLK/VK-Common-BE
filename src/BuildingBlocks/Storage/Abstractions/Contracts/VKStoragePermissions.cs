using System;
namespace VK.Blocks.Storage;

[Flags]
public enum VKStoragePermissions
{
    None = 0,
    Read = 1,
    Write = 2,
    Delete = 4
}
