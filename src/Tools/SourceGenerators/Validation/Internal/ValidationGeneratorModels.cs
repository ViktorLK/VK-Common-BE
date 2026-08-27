using System.Collections.Immutable;

namespace VK.Tools.SourceGenerators.Validation.Internal;

internal enum RuleKind
{
    Required,
    StringLength,
    MinLength,
    MaxLength,
    Range,
    EmailAddress,
    RegularExpression,
    EnumDataType,
    Compare,
    NestedValidate
}

internal sealed record ValidationRuleInfo(
    RuleKind Kind,
    string? CustomErrorMessage = null,
    string? CustomErrorCode = null,
    bool? AllowEmptyStrings = null,
    int? MinLength = null,
    int? MaxLength = null,
    string? Min = null,
    string? Max = null,
    string? Pattern = null,
    string? OtherProperty = null,
    string? EnumTypeName = null);

internal sealed record PropertyValidationInfo(
    string PropertyName,
    string PropertyTypeName,
    bool IsString,
    bool IsNullable,
    bool IsGuid,
    bool IsNumeric,
    bool IsEnum,
    bool IsCollection,
    bool IsSensitive,
    string? ElementTypeName,
    ImmutableArray<ValidationRuleInfo> Rules);

internal sealed record ValidatableTypeInfo(
    string Namespace,
    string TypeName,
    string FullTypeName,
    bool IsValueType,
    int CascadeMode,
    ImmutableArray<PropertyValidationInfo> Properties);
