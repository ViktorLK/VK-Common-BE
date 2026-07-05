using System;

namespace VK.Blocks.Core;

/// <summary>
/// この partial record struct が Source Generator によって強く型付けされた ID (Strongly-Typed ID) として自動生成されることをマークします。
/// 基本プロパティ、比較インターフェース、JsonConverter、ValueConverter (EF Core)、および TypeConverter が自動的に生成されます。
/// </summary>
[AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public sealed class VKStronglyTypedIdAttribute : Attribute
{
}
