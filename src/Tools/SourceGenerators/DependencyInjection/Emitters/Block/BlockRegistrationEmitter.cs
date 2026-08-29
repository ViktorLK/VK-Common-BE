using System;
using Microsoft.CodeAnalysis;
using VK.Tools.SourceGenerators.DependencyInjection.Models;
using VK.Tools.SourceGenerators.Extensions;

namespace VK.Tools.SourceGenerators.DependencyInjection.Emitters.Block;

internal static class BlockRegistrationEmitter
{
    public static void Emit(SourceProductionContext ctx, BlockTargetInfo target)
    {
        var sb = SourceCodeBuilder.CreateWithHeader();
        sb.AppendLine("using System;");
        sb.AppendLine("using Microsoft.Extensions.Configuration;");
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection.Extensions;");
        sb.AppendLine("using Microsoft.Extensions.Options;");
        sb.AppendLine("using VK.Blocks.Core;");
        if (target.HasPersistEntities)
        {
            sb.AppendLine("using VK.Blocks.Persistence.EFCore;");
        }
        sb.AppendLine($"using {target.Namespace};");
        sb.AppendLine();
        sb.AppendLine($"namespace {target.Namespace}.Common.DependencyInjection.Internal;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Handles the core DI registration for the {target.BlockName} block.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"internal static partial class {target.BlockName}BlockRegistration");
        sb.AppendLine("{");
        sb.AppendLine($"    internal static IVK{target.BlockName}Builder Register(");
        sb.AppendLine("        IServiceCollection services,");
        sb.AppendLine("        IConfiguration configuration,");
        sb.AppendLine($"        Func<VK{target.BlockName}Options, VK{target.BlockName}Options>? transform = null)");
        sb.AppendLine("    {");
        sb.AppendLine("        // 1. Check-Self");
        sb.AppendLine($"        if (services.IsVKBlockRegistered<{target.ClassName}>())");
        sb.AppendLine("        {");
        sb.AppendLine($"            return new {target.BlockName}BlockBuilder(services, configuration);");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        // 2. Options Registration");
        sb.AppendLine($"        VK{target.BlockName}Options options = services.AddVKBlockOptions<VK{target.BlockName}Options>(configuration, transform);");
        sb.AppendLine();
        sb.AppendLine("        // 3. Mark-Self");
        sb.AppendLine($"        services.AddVKBlockMarker<{target.ClassName}>();");
        sb.AppendLine();
        sb.AppendLine("        // 4. Validate Options");
        sb.AppendLine($"        services.TryAddEnumerableSingleton<IValidateOptions<VK{target.BlockName}Options>, {target.ClassName}.OptionsValidator>();");
        sb.AppendLine();
        sb.AppendLine("        // 4b. Options Provider Registration");
        sb.AppendLine($"        services.TryAddSingleton<IVK{target.BlockName}OptionsProvider, Default{target.BlockName}OptionsProvider>();");
        sb.AppendLine();
        sb.AppendLine($"        var builder = new {target.BlockName}BlockBuilder(services, configuration);");
        sb.AppendLine();
        sb.AppendLine("        // 5. Feature Toggle");
        if (target.Toggleable)
        {
            sb.AppendLine("        if (!options.Enabled)");
            sb.AppendLine("        {");
            sb.AppendLine("            return builder;");
            sb.AppendLine("        }");
            sb.AppendLine();
        }

        if (target.HasPersistEntities)
        {
            sb.AppendLine("        // 5b. Auto-Generated Persistence Pipeline");
            sb.AppendLine("        services.AddGeneratedModelContributors();");
            sb.AppendLine("        services.AddGeneratedAggregateRepositories();");
            sb.AppendLine();
        }

        // 6. Custom Hook
        sb.AppendLine("        // 6. Custom Hook");
        sb.AppendLine($"        {target.ClassName}.Register(builder);");
        sb.AppendLine("        return builder;");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        ctx.AddSource($"{target.BlockName}BlockRegistration.g.cs", sb.ToString());
    }
}
