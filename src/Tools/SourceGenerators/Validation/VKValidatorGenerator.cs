using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VK.Tools.SourceGenerators.Extensions;
using VK.Tools.SourceGenerators.Utilities;
using VK.Tools.SourceGenerators.Validation.Internal;

namespace VK.Tools.SourceGenerators.Validation;

/// <summary>
/// Incremental Source Generator that produces zero-reflection, compile-time <c>IVKValidator&lt;T&gt;</c>
/// implementations for models decorated with validation attributes (DataAnnotations and VK validation attributes).
/// </summary>
[Generator]
public sealed class VKValidatorGenerator : IIncrementalGenerator
{
    private const string VKValidateAttributeName = "VKValidateAttribute";
    private const string VKValidateAttributeFullName = "VK.Blocks.Validation.VKValidateAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var assemblyName = context.CompilationProvider.Select(static (c, _) => c.AssemblyName);

        var validatableTargets = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: IsCandidateForValidation,
                transform: GetValidatableTarget)
            .WhereNotNull();

        var combined = validatableTargets.Collect().Combine(assemblyName);

        context.RegisterSourceOutput(combined, (spc, pair) => Execute(spc, pair.Left, pair.Right, this.GetType()));
    }

    private static bool IsCandidateForValidation(SyntaxNode node, CancellationToken _)
    {
        return node is ClassDeclarationSyntax { AttributeLists.Count: > 0 } ||
               node is RecordDeclarationSyntax { AttributeLists.Count: > 0 } ||
               node is StructDeclarationSyntax { AttributeLists.Count: > 0 };
    }

    private static ValidatableTypeInfo? GetValidatableTarget(GeneratorSyntaxContext context, CancellationToken ct)
    {
        var symbol = context.SemanticModel.GetDeclaredSymbol(context.Node, ct) as INamedTypeSymbol;
        if (symbol is null || symbol.IsAbstract || symbol.TypeKind == TypeKind.Interface)
        {
            return null;
        }

        var typeAttributes = symbol.GetAttributes();
        var hasVKValidateOnType = typeAttributes.Any(a =>
            a.AttributeClass?.Name == VKValidateAttributeName ||
            a.AttributeClass?.ToDisplayString() == VKValidateAttributeFullName);

        int cascadeMode = 0; // Continue
        if (hasVKValidateOnType)
        {
            var vkValidateAttr = typeAttributes.FirstOrDefault(a =>
                a.AttributeClass?.Name == VKValidateAttributeName ||
                a.AttributeClass?.ToDisplayString() == VKValidateAttributeFullName);

            if (vkValidateAttr is not null)
            {
                var cascadeArg = vkValidateAttr.NamedArguments.FirstOrDefault(x => x.Key == "CascadeMode");
                if (cascadeArg.Value.Value is byte bVal)
                {
                    cascadeMode = bVal;
                }
                else if (cascadeArg.Value.Value is int iVal)
                {
                    cascadeMode = iVal;
                }
            }
        }

        var propertyTargets = new List<PropertyValidationInfo>();

        var properties = symbol.GetMembers().OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public && !p.IsStatic && !p.IsIndexer && p.GetMethod is not null)
            .ToList();

        foreach (var prop in properties)
        {
            var propAttrs = prop.GetAttributes();
            var isSensitive = propAttrs.Any(a => a.AttributeClass?.Name.Contains("SensitiveData") == true);
            var ruleTargets = new List<ValidationRuleInfo>();

            foreach (var attr in propAttrs)
            {
                var attrName = attr.AttributeClass?.Name ?? string.Empty;
                var customMessage = GetNamedOrCtorStringArgument(attr, "ErrorMessage");

                if (attrName is "RequiredAttribute" or "Required")
                {
                    var allowEmpty = GetNamedArgument<bool>(attr, "AllowEmptyStrings");
                    ruleTargets.Add(new ValidationRuleInfo(
                        RuleKind.Required,
                        CustomErrorMessage: customMessage,
                        AllowEmptyStrings: allowEmpty));
                }
                else if (attrName is "StringLengthAttribute" or "StringLength")
                {
                    int max = attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is int maxVal ? maxVal : int.MaxValue;
                    int min = GetNamedArgument<int>(attr, "MinimumLength") ?? 0;
                    ruleTargets.Add(new ValidationRuleInfo(
                        RuleKind.StringLength,
                        CustomErrorMessage: customMessage,
                        MinLength: min,
                        MaxLength: max));
                }
                else if (attrName is "MinLengthAttribute" or "MinLength")
                {
                    int min = attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is int minVal ? minVal : 0;
                    ruleTargets.Add(new ValidationRuleInfo(
                        RuleKind.MinLength,
                        CustomErrorMessage: customMessage,
                        MinLength: min));
                }
                else if (attrName is "MaxLengthAttribute" or "MaxLength")
                {
                    int max = attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is int maxVal ? maxVal : int.MaxValue;
                    ruleTargets.Add(new ValidationRuleInfo(
                        RuleKind.MaxLength,
                        CustomErrorMessage: customMessage,
                        MaxLength: max));
                }
                else if (attrName is "RangeAttribute" or "Range")
                {
                    string? min = null;
                    string? max = null;
                    if (attr.ConstructorArguments.Length >= 2)
                    {
                        min = attr.ConstructorArguments[0].Value?.ToString();
                        max = attr.ConstructorArguments[1].Value?.ToString();
                    }
                    ruleTargets.Add(new ValidationRuleInfo(
                        RuleKind.Range,
                        CustomErrorMessage: customMessage,
                        Min: min,
                        Max: max));
                }
                else if (attrName is "EmailAddressAttribute" or "EmailAddress")
                {
                    ruleTargets.Add(new ValidationRuleInfo(
                        RuleKind.EmailAddress,
                        CustomErrorMessage: customMessage));
                }
                else if (attrName is "RegularExpressionAttribute" or "RegularExpression")
                {
                    string? pattern = attr.ConstructorArguments.Length > 0 ? attr.ConstructorArguments[0].Value as string : null;
                    ruleTargets.Add(new ValidationRuleInfo(
                        RuleKind.RegularExpression,
                        CustomErrorMessage: customMessage,
                        Pattern: pattern));
                }
                else if (attrName is "EnumDataTypeAttribute" or "EnumDataType")
                {
                    string? enumType = null;
                    if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is ITypeSymbol typeSym)
                    {
                        enumType = typeSym.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    }
                    ruleTargets.Add(new ValidationRuleInfo(
                        RuleKind.EnumDataType,
                        CustomErrorMessage: customMessage,
                        EnumTypeName: enumType));
                }
                else if (attrName is "CompareAttribute" or "Compare")
                {
                    string? other = attr.ConstructorArguments.Length > 0 ? attr.ConstructorArguments[0].Value as string : null;
                    ruleTargets.Add(new ValidationRuleInfo(
                        RuleKind.Compare,
                        CustomErrorMessage: customMessage,
                        OtherProperty: other));
                }
                else if (attrName is VKValidateAttributeName or "ValidateAttribute")
                {
                    ruleTargets.Add(new ValidationRuleInfo(
                        RuleKind.NestedValidate,
                        CustomErrorMessage: customMessage));
                }
            }

            if (ruleTargets.Count > 0)
            {
                var isString = prop.Type.SpecialType == SpecialType.System_String;
                var isNullable = prop.NullableAnnotation == NullableAnnotation.Annotated ||
                                 prop.Type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
                var isGuid = prop.Type.ToDisplayString().Contains("Guid");
                var isNumeric = IsNumericType(prop.Type);
                var isEnum = prop.Type.TypeKind == TypeKind.Enum || (prop.Type is INamedTypeSymbol nts && nts.TypeArguments.Length == 1 && nts.TypeArguments[0].TypeKind == TypeKind.Enum);
                var isCollection = !isString && (prop.Type.AllInterfaces.Any(i => i.Name == "IEnumerable") || prop.Type.Name == "IEnumerable");

                string? elementTypeName = null;
                if (isCollection && prop.Type is INamedTypeSymbol namedCol && namedCol.TypeArguments.Length == 1)
                {
                    elementTypeName = namedCol.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                }

                propertyTargets.Add(new PropertyValidationInfo(
                    PropertyName: prop.Name,
                    PropertyTypeName: prop.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    IsString: isString,
                    IsNullable: isNullable,
                    IsGuid: isGuid,
                    IsNumeric: isNumeric,
                    IsEnum: isEnum,
                    IsCollection: isCollection,
                    IsSensitive: isSensitive,
                    ElementTypeName: elementTypeName,
                    Rules: [.. ruleTargets]));
            }
        }

        // If not explicitly marked with VKValidate and has no property rules, ignore
        if (!hasVKValidateOnType && propertyTargets.Count == 0)
        {
            return null;
        }

        return new ValidatableTypeInfo(
            Namespace: symbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : symbol.ContainingNamespace.ToDisplayString(),
            TypeName: symbol.Name,
            FullTypeName: symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            IsValueType: symbol.IsValueType,
            CascadeMode: cascadeMode,
            Properties: [.. propertyTargets]);
    }

    private static void Execute(
        SourceProductionContext context,
        ImmutableArray<ValidatableTypeInfo> targets,
        string? assemblyName,
        Type generatorType)
    {
        if (!VKBlockGeneratorGuard.ShouldExecute(generatorType, assemblyName))
        {
            return;
        }

        var distinctTargets = targets
            .GroupBy(t => t.FullTypeName)
            .Select(g => g.First())
            .OrderBy(t => t.FullTypeName)
            .ToList();

        if (distinctTargets.Count == 0)
        {
            return;
        }

        foreach (var target in distinctTargets)
        {
            ValidatorSourceEmitter.EmitValidator(context, target);
        }

        ValidatorSourceEmitter.EmitDIExtensions(context, distinctTargets);
    }

    private static bool IsNumericType(ITypeSymbol type)
    {
        var unwrapped = type is INamedTypeSymbol nts && nts.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T && nts.TypeArguments.Length == 1
            ? nts.TypeArguments[0]
            : type;

        return unwrapped.SpecialType is SpecialType.System_Byte
            or SpecialType.System_SByte
            or SpecialType.System_Int16
            or SpecialType.System_UInt16
            or SpecialType.System_Int32
            or SpecialType.System_UInt32
            or SpecialType.System_Int64
            or SpecialType.System_UInt64
            or SpecialType.System_Single
            or SpecialType.System_Double
            or SpecialType.System_Decimal;
    }

    private static string? GetNamedOrCtorStringArgument(AttributeData attr, string name)
    {
        var named = attr.NamedArguments.FirstOrDefault(x => x.Key == name);
        if (named.Value.Value is string s)
        {
            return s;
        }
        return null;
    }

    private static T? GetNamedArgument<T>(AttributeData attr, string name) where T : struct
    {
        var named = attr.NamedArguments.FirstOrDefault(x => x.Key == name);
        if (named.Value.Value is T val)
        {
            return val;
        }
        return null;
    }
}
