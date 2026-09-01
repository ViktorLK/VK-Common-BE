using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using VK.Tools.SourceGenerators.Extensions;

namespace VK.Tools.SourceGenerators.Validation.Internal;

internal static class ValidatorSourceEmitter
{
    public static void EmitValidator(SourceProductionContext ctx, ValidatableTypeInfo target)
    {
        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine("using System.Text.RegularExpressions;");
        sb.AppendLine("using System.Threading;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using VK.Blocks.Validation;");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(target.Namespace))
        {
            sb.AppendLine($"namespace {target.Namespace};");
            sb.AppendLine();
        }

        var validatorClassName = $"{target.TypeName}Validator";
        var isStopCascade = target.CascadeMode == 1;

        sb.AppendLine($"[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = \"Source-generated compile-time validator.\")]");
        sb.AppendLine($"public sealed partial class {validatorClassName} : global::VK.Blocks.Validation.IVKValidator<{target.FullTypeName}>");
        sb.AppendLine("{");

        // Collect all regexes needed
        var hasEmail = target.Properties.Any(p => p.Rules.Any(r => r.Kind == RuleKind.EmailAddress));
        if (hasEmail)
        {
            sb.AppendLine("    private static readonly global::System.Text.RegularExpressions.Regex _emailRegex = new global::System.Text.RegularExpressions.Regex(@\"^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$\", global::System.Text.RegularExpressions.RegexOptions.Compiled, global::System.TimeSpan.FromMilliseconds(100));");
            sb.AppendLine();
        }

        var regexRules = target.Properties
            .SelectMany(p => p.Rules.Where(r => r.Kind == RuleKind.RegularExpression && !string.IsNullOrEmpty(r.Pattern)).Select(r => (p.PropertyName, r.Pattern!)))
            .ToList();

        for (int i = 0; i < regexRules.Count; i++)
        {
            var (propName, pattern) = regexRules[i];
            var escapedPattern = pattern.Replace("\"", "\"\"");
            sb.AppendLine($"    private static readonly global::System.Text.RegularExpressions.Regex _regex_{propName}_{i} = new global::System.Text.RegularExpressions.Regex(@\"{escapedPattern}\", global::System.Text.RegularExpressions.RegexOptions.Compiled, global::System.TimeSpan.FromMilliseconds(100));");
        }
        if (regexRules.Count > 0)
        {
            sb.AppendLine();
        }

        var hasNested = target.Properties.Any(p => p.Rules.Any(r => r.Kind == RuleKind.NestedValidate));
        var asyncKeyword = hasNested ? "async " : "";

        sb.AppendLine($"    public {asyncKeyword}global::System.Threading.Tasks.Task<global::VK.Blocks.Validation.VKValidationResult> ValidateAsync({target.FullTypeName} model, global::System.Threading.CancellationToken ct = default)");
        sb.AppendLine("    {");

        if (!target.IsValueType)
        {
            sb.AppendLine("        if (model is null)");
            sb.AppendLine("        {");
            if (hasNested)
            {
                sb.AppendLine("            return global::VK.Blocks.Validation.VKValidationResult.Failure(string.Empty, \"Model cannot be null.\");");
            }
            else
            {
                sb.AppendLine("            return global::System.Threading.Tasks.Task.FromResult(global::VK.Blocks.Validation.VKValidationResult.Failure(string.Empty, \"Model cannot be null.\"));");
            }
            sb.AppendLine("        }");
            sb.AppendLine();
        }

        sb.AppendLine("        var errors = new global::System.Collections.Generic.List<global::VK.Blocks.Validation.VKValidationError>();");
        sb.AppendLine();

        int regexCounter = 0;

        foreach (var prop in target.Properties)
        {
            if (prop.Rules.IsEmpty)
                continue;

            sb.AppendLine($"        // Rules for {prop.PropertyName}");
            var attemptedExpr = prop.IsSensitive ? "\"******\"" : $"model.{prop.PropertyName}";

            foreach (var rule in prop.Rules)
            {
                switch (rule.Kind)
                {
                    case RuleKind.Required:
                        EmitRequiredCheck(sb, prop, rule, attemptedExpr, isStopCascade, hasNested);
                        break;

                    case RuleKind.StringLength:
                        EmitStringLengthCheck(sb, prop, rule, attemptedExpr, isStopCascade, hasNested);
                        break;

                    case RuleKind.MinLength:
                        EmitMinLengthCheck(sb, prop, rule, attemptedExpr, isStopCascade, hasNested);
                        break;

                    case RuleKind.MaxLength:
                        EmitMaxLengthCheck(sb, prop, rule, attemptedExpr, isStopCascade, hasNested);
                        break;

                    case RuleKind.Range:
                        EmitRangeCheck(sb, prop, rule, attemptedExpr, isStopCascade, hasNested);
                        break;

                    case RuleKind.EmailAddress:
                        EmitEmailCheck(sb, prop, rule, attemptedExpr, isStopCascade, hasNested);
                        break;

                    case RuleKind.RegularExpression:
                        EmitRegexCheck(sb, prop, rule, attemptedExpr, regexCounter++, isStopCascade, hasNested);
                        break;

                    case RuleKind.EnumDataType:
                        EmitEnumCheck(sb, prop, rule, attemptedExpr, isStopCascade, hasNested);
                        break;

                    case RuleKind.Compare:
                        EmitCompareCheck(sb, prop, rule, attemptedExpr, isStopCascade, hasNested);
                        break;

                    case RuleKind.NestedValidate:
                        EmitNestedCheck(sb, prop, isStopCascade);
                        break;
                }
            }

            sb.AppendLine();
        }

        if (hasNested)
        {
            sb.AppendLine("        return errors.Count == 0 ? global::VK.Blocks.Validation.VKValidationResult.Success() : global::VK.Blocks.Validation.VKValidationResult.Failure(errors);");
        }
        else
        {
            sb.AppendLine("        return global::System.Threading.Tasks.Task.FromResult(errors.Count == 0 ? global::VK.Blocks.Validation.VKValidationResult.Success() : global::VK.Blocks.Validation.VKValidationResult.Failure(errors));");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        ctx.AddSource($"{target.TypeName}Validator.g.cs", sb.ToString());
    }

    private static void EmitRequiredCheck(StringBuilder sb, PropertyValidationInfo prop, ValidationRuleInfo rule, string attemptedExpr, bool isStopCascade, bool isAsyncMethod)
    {
        var msg = rule.CustomErrorMessage ?? $"{prop.PropertyName} is required.";
        var code = rule.CustomErrorCode ?? "global::VK.Blocks.Validation.VKValidationCodes.Required";

        string condition;
        if (prop.IsString)
        {
            condition = rule.AllowEmptyStrings == true
                ? $"model.{prop.PropertyName} is null"
                : $"string.IsNullOrWhiteSpace(model.{prop.PropertyName})";
        }
        else if (prop.IsGuid)
        {
            condition = prop.IsNullable
                ? $"!model.{prop.PropertyName}.HasValue || model.{prop.PropertyName}.Value == global::System.Guid.Empty"
                : $"model.{prop.PropertyName} == global::System.Guid.Empty";
        }
        else if (prop.IsCollection)
        {
            condition = $"model.{prop.PropertyName} is null || !global::System.Linq.Enumerable.Any(model.{prop.PropertyName})";
        }
        else if (prop.IsNullable)
        {
            condition = $"model.{prop.PropertyName} is null";
        }
        else
        {
            condition = $"model.{prop.PropertyName} == default";
        }

        sb.AppendLine($"        if ({condition})");
        sb.AppendLine("        {");
        sb.AppendLine($"            errors.Add(new global::VK.Blocks.Validation.VKValidationError(\"{prop.PropertyName}\", \"{EscapeString(msg)}\", {code}, global::VK.Blocks.Validation.VKValidationSeverity.Error, {attemptedExpr}));");
        if (isStopCascade)
        {
            sb.AppendLine(isAsyncMethod
                ? "            return global::VK.Blocks.Validation.VKValidationResult.Failure(errors);"
                : "            return global::System.Threading.Tasks.Task.FromResult(global::VK.Blocks.Validation.VKValidationResult.Failure(errors));");
        }
        sb.AppendLine("        }");
    }

    private static void EmitStringLengthCheck(StringBuilder sb, PropertyValidationInfo prop, ValidationRuleInfo rule, string attemptedExpr, bool isStopCascade, bool isAsyncMethod)
    {
        var min = rule.MinLength ?? 0;
        var max = rule.MaxLength ?? int.MaxValue;
        var msg = rule.CustomErrorMessage ?? (min > 0
            ? $"{prop.PropertyName} must be between {min} and {max} characters."
            : $"{prop.PropertyName} must not exceed {max} characters.");
        var code = rule.CustomErrorCode ?? "global::VK.Blocks.Validation.VKValidationCodes.Length";

        string lengthCondition;
        if (min > 0)
        {
            lengthCondition = $"model.{prop.PropertyName}.Length < {min} || model.{prop.PropertyName}.Length > {max}";
        }
        else
        {
            lengthCondition = $"model.{prop.PropertyName}.Length > {max}";
        }

        sb.AppendLine($"        if (model.{prop.PropertyName} is not null && ({lengthCondition}))");
        sb.AppendLine("        {");
        sb.AppendLine($"            errors.Add(new global::VK.Blocks.Validation.VKValidationError(\"{prop.PropertyName}\", \"{EscapeString(msg)}\", {code}, global::VK.Blocks.Validation.VKValidationSeverity.Error, {attemptedExpr}));");
        if (isStopCascade)
        {
            sb.AppendLine(isAsyncMethod
                ? "            return global::VK.Blocks.Validation.VKValidationResult.Failure(errors);"
                : "            return global::System.Threading.Tasks.Task.FromResult(global::VK.Blocks.Validation.VKValidationResult.Failure(errors));");
        }
        sb.AppendLine("        }");
    }

    private static void EmitMinLengthCheck(StringBuilder sb, PropertyValidationInfo prop, ValidationRuleInfo rule, string attemptedExpr, bool isStopCascade, bool isAsyncMethod)
    {
        var min = rule.MinLength ?? 0;
        var msg = rule.CustomErrorMessage ?? $"{prop.PropertyName} must have at least {min} elements.";
        var code = rule.CustomErrorCode ?? (prop.IsString ? "global::VK.Blocks.Validation.VKValidationCodes.Length" : "global::VK.Blocks.Validation.VKValidationCodes.Collection");

        var lengthExpr = prop.IsString ? $"model.{prop.PropertyName}.Length" : $"global::System.Linq.Enumerable.Count(model.{prop.PropertyName})";

        sb.AppendLine($"        if (model.{prop.PropertyName} is not null && {lengthExpr} < {min})");
        sb.AppendLine("        {");
        sb.AppendLine($"            errors.Add(new global::VK.Blocks.Validation.VKValidationError(\"{prop.PropertyName}\", \"{EscapeString(msg)}\", {code}, global::VK.Blocks.Validation.VKValidationSeverity.Error, {attemptedExpr}));");
        if (isStopCascade)
        {
            sb.AppendLine(isAsyncMethod
                ? "            return global::VK.Blocks.Validation.VKValidationResult.Failure(errors);"
                : "            return global::System.Threading.Tasks.Task.FromResult(global::VK.Blocks.Validation.VKValidationResult.Failure(errors));");
        }
        sb.AppendLine("        }");
    }

    private static void EmitMaxLengthCheck(StringBuilder sb, PropertyValidationInfo prop, ValidationRuleInfo rule, string attemptedExpr, bool isStopCascade, bool isAsyncMethod)
    {
        var max = rule.MaxLength ?? int.MaxValue;
        var msg = rule.CustomErrorMessage ?? $"{prop.PropertyName} must have at most {max} elements.";
        var code = rule.CustomErrorCode ?? (prop.IsString ? "global::VK.Blocks.Validation.VKValidationCodes.Length" : "global::VK.Blocks.Validation.VKValidationCodes.Collection");

        var lengthExpr = prop.IsString ? $"model.{prop.PropertyName}.Length" : $"global::System.Linq.Enumerable.Count(model.{prop.PropertyName})";

        sb.AppendLine($"        if (model.{prop.PropertyName} is not null && {lengthExpr} > {max})");
        sb.AppendLine("        {");
        sb.AppendLine($"            errors.Add(new global::VK.Blocks.Validation.VKValidationError(\"{prop.PropertyName}\", \"{EscapeString(msg)}\", {code}, global::VK.Blocks.Validation.VKValidationSeverity.Error, {attemptedExpr}));");
        if (isStopCascade)
        {
            sb.AppendLine(isAsyncMethod
                ? "            return global::VK.Blocks.Validation.VKValidationResult.Failure(errors);"
                : "            return global::System.Threading.Tasks.Task.FromResult(global::VK.Blocks.Validation.VKValidationResult.Failure(errors));");
        }
        sb.AppendLine("        }");
    }

    private static void EmitRangeCheck(StringBuilder sb, PropertyValidationInfo prop, ValidationRuleInfo rule, string attemptedExpr, bool isStopCascade, bool isAsyncMethod)
    {
        var min = rule.Min ?? "0";
        var max = rule.Max ?? "0";
        var msg = rule.CustomErrorMessage ?? $"{prop.PropertyName} must be between {min} and {max}.";
        var code = rule.CustomErrorCode ?? "global::VK.Blocks.Validation.VKValidationCodes.Range";

        string condition;
        if (prop.IsNullable)
        {
            condition = $"model.{prop.PropertyName}.HasValue && (model.{prop.PropertyName}.Value < {min} || model.{prop.PropertyName}.Value > {max})";
        }
        else
        {
            condition = $"model.{prop.PropertyName} < {min} || model.{prop.PropertyName} > {max}";
        }

        sb.AppendLine($"        if ({condition})");
        sb.AppendLine("        {");
        sb.AppendLine($"            errors.Add(new global::VK.Blocks.Validation.VKValidationError(\"{prop.PropertyName}\", \"{EscapeString(msg)}\", {code}, global::VK.Blocks.Validation.VKValidationSeverity.Error, {attemptedExpr}));");
        if (isStopCascade)
        {
            sb.AppendLine(isAsyncMethod
                ? "            return global::VK.Blocks.Validation.VKValidationResult.Failure(errors);"
                : "            return global::System.Threading.Tasks.Task.FromResult(global::VK.Blocks.Validation.VKValidationResult.Failure(errors));");
        }
        sb.AppendLine("        }");
    }

    private static void EmitEmailCheck(StringBuilder sb, PropertyValidationInfo prop, ValidationRuleInfo rule, string attemptedExpr, bool isStopCascade, bool isAsyncMethod)
    {
        var msg = rule.CustomErrorMessage ?? $"{prop.PropertyName} must be a valid email address.";
        var code = rule.CustomErrorCode ?? "global::VK.Blocks.Validation.VKValidationCodes.Email";

        sb.AppendLine($"        if (!string.IsNullOrEmpty(model.{prop.PropertyName}) && !_emailRegex.IsMatch(model.{prop.PropertyName}))");
        sb.AppendLine("        {");
        sb.AppendLine($"            errors.Add(new global::VK.Blocks.Validation.VKValidationError(\"{prop.PropertyName}\", \"{EscapeString(msg)}\", {code}, global::VK.Blocks.Validation.VKValidationSeverity.Error, {attemptedExpr}));");
        if (isStopCascade)
        {
            sb.AppendLine(isAsyncMethod
                ? "            return global::VK.Blocks.Validation.VKValidationResult.Failure(errors);"
                : "            return global::System.Threading.Tasks.Task.FromResult(global::VK.Blocks.Validation.VKValidationResult.Failure(errors));");
        }
        sb.AppendLine("        }");
    }

    private static void EmitRegexCheck(StringBuilder sb, PropertyValidationInfo prop, ValidationRuleInfo rule, string attemptedExpr, int regexIndex, bool isStopCascade, bool isAsyncMethod)
    {
        var msg = rule.CustomErrorMessage ?? $"{prop.PropertyName} format is invalid.";
        var code = rule.CustomErrorCode ?? "global::VK.Blocks.Validation.VKValidationCodes.Pattern";

        sb.AppendLine($"        if (!string.IsNullOrEmpty(model.{prop.PropertyName}) && !_regex_{prop.PropertyName}_{regexIndex}.IsMatch(model.{prop.PropertyName}))");
        sb.AppendLine("        {");
        sb.AppendLine($"            errors.Add(new global::VK.Blocks.Validation.VKValidationError(\"{prop.PropertyName}\", \"{EscapeString(msg)}\", {code}, global::VK.Blocks.Validation.VKValidationSeverity.Error, {attemptedExpr}));");
        if (isStopCascade)
        {
            sb.AppendLine(isAsyncMethod
                ? "            return global::VK.Blocks.Validation.VKValidationResult.Failure(errors);"
                : "            return global::System.Threading.Tasks.Task.FromResult(global::VK.Blocks.Validation.VKValidationResult.Failure(errors));");
        }
        sb.AppendLine("        }");
    }

    private static void EmitEnumCheck(StringBuilder sb, PropertyValidationInfo prop, ValidationRuleInfo rule, string attemptedExpr, bool isStopCascade, bool isAsyncMethod)
    {
        var enumType = rule.EnumTypeName ?? prop.PropertyTypeName;
        var msg = rule.CustomErrorMessage ?? $"{prop.PropertyName} is not a valid enum value.";
        var code = rule.CustomErrorCode ?? "global::VK.Blocks.Validation.VKValidationCodes.Enum";

        string condition;
        if (prop.IsNullable)
        {
            condition = $"model.{prop.PropertyName}.HasValue && !global::System.Enum.IsDefined(typeof({enumType}), model.{prop.PropertyName}.Value)";
        }
        else
        {
            condition = $"!global::System.Enum.IsDefined(typeof({enumType}), model.{prop.PropertyName})";
        }

        sb.AppendLine($"        if ({condition})");
        sb.AppendLine("        {");
        sb.AppendLine($"            errors.Add(new global::VK.Blocks.Validation.VKValidationError(\"{prop.PropertyName}\", \"{EscapeString(msg)}\", {code}, global::VK.Blocks.Validation.VKValidationSeverity.Error, {attemptedExpr}));");
        if (isStopCascade)
        {
            sb.AppendLine(isAsyncMethod
                ? "            return global::VK.Blocks.Validation.VKValidationResult.Failure(errors);"
                : "            return global::System.Threading.Tasks.Task.FromResult(global::VK.Blocks.Validation.VKValidationResult.Failure(errors));");
        }
        sb.AppendLine("        }");
    }

    private static void EmitCompareCheck(StringBuilder sb, PropertyValidationInfo prop, ValidationRuleInfo rule, string attemptedExpr, bool isStopCascade, bool isAsyncMethod)
    {
        var other = rule.OtherProperty ?? string.Empty;
        var msg = rule.CustomErrorMessage ?? $"{prop.PropertyName} and {other} do not match.";
        var code = rule.CustomErrorCode ?? "global::VK.Blocks.Validation.VKValidationCodes.Custom";

        sb.AppendLine($"        if (!global::System.Collections.Generic.EqualityComparer<{prop.PropertyTypeName}>.Default.Equals(model.{prop.PropertyName}, model.{other}))");
        sb.AppendLine("        {");
        sb.AppendLine($"            errors.Add(new global::VK.Blocks.Validation.VKValidationError(\"{prop.PropertyName}\", \"{EscapeString(msg)}\", {code}, global::VK.Blocks.Validation.VKValidationSeverity.Error, {attemptedExpr}));");
        if (isStopCascade)
        {
            sb.AppendLine(isAsyncMethod
                ? "            return global::VK.Blocks.Validation.VKValidationResult.Failure(errors);"
                : "            return global::System.Threading.Tasks.Task.FromResult(global::VK.Blocks.Validation.VKValidationResult.Failure(errors));");
        }
        sb.AppendLine("        }");
    }

    private static void EmitNestedCheck(StringBuilder sb, PropertyValidationInfo prop, bool isStopCascade)
    {
        var childType = prop.ElementTypeName ?? prop.PropertyTypeName.TrimEnd('?');
        var childValidatorName = $"{childType.Substring(childType.LastIndexOf('.') + 1)}Validator";

        if (prop.IsCollection)
        {
            sb.AppendLine($"        if (model.{prop.PropertyName} is not null)");
            sb.AppendLine("        {");
            sb.AppendLine($"            var childValidator = new {childValidatorName}();");
            sb.AppendLine("            int elemIndex = 0;");
            sb.AppendLine($"            foreach (var elem in model.{prop.PropertyName})");
            sb.AppendLine("            {");
            sb.AppendLine("                if (elem is not null)");
            sb.AppendLine("                {");
            sb.AppendLine("                    var childResult = await childValidator.ValidateAsync(elem, ct).ConfigureAwait(false);");
            sb.AppendLine("                    if (!childResult.IsValid)");
            sb.AppendLine("                    {");
            sb.AppendLine("                        foreach (var err in childResult.Errors)");
            sb.AppendLine("                        {");
            sb.AppendLine($"                            var path = string.IsNullOrEmpty(err.PropertyName) ? $\"{prop.PropertyName}[{{elemIndex}}]\" : $\"{prop.PropertyName}[{{elemIndex}}].{{err.PropertyName}}\";");
            sb.AppendLine("                            errors.Add(new global::VK.Blocks.Validation.VKValidationError(path, err.ErrorMessage, err.ErrorCode, err.Severity, err.AttemptedValue, err.Metadata));");
            sb.AppendLine("                        }");
            if (isStopCascade)
            {
                sb.AppendLine("                        return global::VK.Blocks.Validation.VKValidationResult.Failure(errors);");
            }
            sb.AppendLine("                    }");
            sb.AppendLine("                }");
            sb.AppendLine("                elemIndex++;");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
        }
        else
        {
            sb.AppendLine($"        if (model.{prop.PropertyName} is not null)");
            sb.AppendLine("        {");
            sb.AppendLine($"            var childValidator = new {childValidatorName}();");
            sb.AppendLine($"            var childResult = await childValidator.ValidateAsync(model.{prop.PropertyName}, ct).ConfigureAwait(false);");
            sb.AppendLine("            if (!childResult.IsValid)");
            sb.AppendLine("            {");
            sb.AppendLine("                foreach (var err in childResult.Errors)");
            sb.AppendLine("                {");
            sb.AppendLine($"                    var path = string.IsNullOrEmpty(err.PropertyName) ? \"{prop.PropertyName}\" : $\"{prop.PropertyName}.{{err.PropertyName}}\";");
            sb.AppendLine("                    errors.Add(new global::VK.Blocks.Validation.VKValidationError(path, err.ErrorMessage, err.ErrorCode, err.Severity, err.AttemptedValue, err.Metadata));");
            sb.AppendLine("                }");
            if (isStopCascade)
            {
                sb.AppendLine("                return global::VK.Blocks.Validation.VKValidationResult.Failure(errors);");
            }
            sb.AppendLine("            }");
            sb.AppendLine("        }");
        }
    }

    public static void EmitDIExtensions(SourceProductionContext ctx, IReadOnlyList<ValidatableTypeInfo> targets)
    {
        if (targets.Count == 0)
            return;

        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection.Extensions;");
        sb.AppendLine("using VK.Blocks.Validation;");
        sb.AppendLine();
        sb.AppendLine("namespace Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine();
        sb.AppendLine("[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = \"Source-generated validator DI registration.\")]");
        sb.AppendLine("public static class VKGeneratedValidationExtensions");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Registers all source-generated compile-time validators into the service collection.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static global::Microsoft.Extensions.DependencyInjection.IServiceCollection AddVKGeneratedValidators(this global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)");
        sb.AppendLine("    {");

        foreach (var target in targets)
        {
            var validatorFqn = string.IsNullOrEmpty(target.Namespace)
                ? $"{target.TypeName}Validator"
                : $"{target.Namespace}.{target.TypeName}Validator";

            sb.AppendLine($"        global::Microsoft.Extensions.DependencyInjection.Extensions.ServiceCollectionDescriptorExtensions.TryAddSingleton<global::VK.Blocks.Validation.IVKValidator<{target.FullTypeName}>, {validatorFqn}>(services);");
            sb.AppendLine($"        global::Microsoft.Extensions.DependencyInjection.Extensions.ServiceCollectionDescriptorExtensions.TryAddEnumerable(services, global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor.Singleton<global::VK.Blocks.Validation.IVKValidator, {validatorFqn}>());");
        }

        sb.AppendLine("        return services;");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        ctx.AddSource("VKGeneratedValidationExtensions.g.cs", sb.ToString());
    }

    private static string EscapeString(string str)
    {
        return str.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
