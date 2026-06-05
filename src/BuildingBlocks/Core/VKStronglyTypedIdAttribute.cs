using System;

namespace VK.Blocks.Core;

/// <summary>
/// 标记该 partial record struct 应该被 Source Generator 自动生成为强类型 ID (Strongly-Typed ID)。
/// 将自动为其生成基础属性、比较接口、JsonConverter、ValueConverter (EF Core) 以及 TypeConverter。
/// </summary>
[AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public sealed class VKStronglyTypedIdAttribute : Attribute
{
}
