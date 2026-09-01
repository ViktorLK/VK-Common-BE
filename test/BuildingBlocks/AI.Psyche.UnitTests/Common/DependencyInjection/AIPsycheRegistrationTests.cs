using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.UnitTests.Common.DependencyInjection;

// [AP.01] Sealed default for test classes
public sealed class AIPsycheRegistrationTests : VKUnitTestBase
{
    [Fact]
    // [DL.01] Method_Scenario_Expected naming convention
    public void AddVKAIPsycheBlock_WithFullFeatures_ShouldRegisterAllCoreServicesAndBuildServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IVKGuidGenerator>(new VKFakeGuidGenerator());
        services.AddSingleton(GetMock<IVKTokenCounter>().Object);
        services.AddSingleton(GetMock<IVKModelCatalog>().Object);
        services.AddSingleton(GetMock<IVKPromptTemplateEngine>().Object);

        // Prerequisite markers [AP.02]
        services.AddVKBlockMarker<VKCoreBlock>();
        services.AddVKBlockMarker<VKAIBlock>();

        var configuration = new ConfigurationBuilder().Build();

        // Act
        services.AddVKAIPsycheBlock(configuration)
            .AddVKDirective()
            .AddVKEcho()
            .AddVKKnowledge()
            .AddVKPattern()
            .AddVKPersona()
            .AddVKProfile()
            .AddVKSession()
            .AddVKWeaving()
            .AddVKPipeline();

        using var serviceProvider = services.BuildServiceProvider();

        // Assert
        serviceProvider.GetRequiredService<IVKPsycheDirectiveRepository>().Should().NotBeNull();
        serviceProvider.GetRequiredService<IVKEchoStore>().Should().NotBeNull();
        serviceProvider.GetRequiredService<IVKPsycheKnowledgeRepository>().Should().NotBeNull();
        serviceProvider.GetRequiredService<IVKPsychePatternRepository>().Should().NotBeNull();
        serviceProvider.GetRequiredService<IVKPsychePersonaRepository>().Should().NotBeNull();
        serviceProvider.GetRequiredService<IVKPsycheProfileRepository>().Should().NotBeNull();
        serviceProvider.GetRequiredService<IVKPsycheSessionRepository>().Should().NotBeNull();
        serviceProvider.GetRequiredService<IVKPsychePipeline>().Should().NotBeNull();
        serviceProvider.GetRequiredService<IVKPsychePipelineExecutor>().Should().NotBeNull();
        serviceProvider.GetRequiredService<IVKPsycheModelFactory>().Should().NotBeNull();
    }

    [Fact]
    // [DL.01] Method_Scenario_Expected naming convention
    public void AddVKAIPsycheBlock_WhenCalledMultipleTimes_ShouldBeIdempotent()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddVKBlockMarker<VKCoreBlock>();
        services.AddVKBlockMarker<VKAIBlock>();

        var configuration = new ConfigurationBuilder().Build();

        // Act & Assert (should not throw)
        services.AddVKAIPsycheBlock(configuration)
            .AddVKDirective()
            .AddVKEcho();

        services.AddVKAIPsycheBlock(configuration)
            .AddVKDirective()
            .AddVKEcho();

        using var serviceProvider = services.BuildServiceProvider();
        serviceProvider.GetRequiredService<IVKPsycheDirectiveRepository>().Should().NotBeNull();
        serviceProvider.GetRequiredService<IVKEchoStore>().Should().NotBeNull();
    }

    [Fact]
    // [DL.01] Method_Scenario_Expected naming convention
    public void AddVKAIPsycheBlock_WithOptionsTransform_ShouldApplyCustomOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddVKBlockMarker<VKCoreBlock>();
        services.AddVKBlockMarker<VKAIBlock>();

        var configuration = new ConfigurationBuilder().Build();

        // Act
        services.AddVKAIPsycheBlock(configuration)
            .AddVKEcho(transform: options => options with
            {
                TokenBudgetRatio = 0.5,
                AutoSaveHistory = false
            })
            .AddVKDirective(transform: options => options with
            {
                Enabled = true
            })
            .AddVKKnowledge(transform: options => options with
            {
                Enabled = true
            })
            .AddVKPattern(transform: options => options with
            {
                Enabled = true
            })
            .AddVKPersona(transform: options => options with
            {
                Enabled = true
            })
            .AddVKProfile(transform: options => options with
            {
                Enabled = true
            })
            .AddVKSession(transform: options => options with
            {
                Enabled = true
            })
            .AddVKWeaving(transform: options => options with
            {
                DefaultResponseReservedTokens = 4096
            })
            .AddVKPipeline();

        using var serviceProvider = services.BuildServiceProvider();
        var echoOptions = serviceProvider.GetRequiredService<IOptions<VKEchoOptions>>().Value;
        var directiveOptions = serviceProvider.GetRequiredService<IOptions<VKDirectiveOptions>>().Value;
        var knowledgeOptions = serviceProvider.GetRequiredService<IOptions<VKKnowledgeOptions>>().Value;
        var patternOptions = serviceProvider.GetRequiredService<IOptions<VKPatternOptions>>().Value;
        var personaOptions = serviceProvider.GetRequiredService<IOptions<VKPersonaOptions>>().Value;
        var profileOptions = serviceProvider.GetRequiredService<IOptions<VKProfileOptions>>().Value;
        var sessionOptions = serviceProvider.GetRequiredService<IOptions<VKSessionOptions>>().Value;
        var weavingOptions = serviceProvider.GetRequiredService<IOptions<VKWeavingOptions>>().Value;

        // Assert
        echoOptions.TokenBudgetRatio.Should().Be(0.5);
        echoOptions.AutoSaveHistory.Should().BeFalse();
        directiveOptions.Enabled.Should().BeTrue();
        knowledgeOptions.Enabled.Should().BeTrue();
        patternOptions.Enabled.Should().BeTrue();
        personaOptions.Enabled.Should().BeTrue();
        profileOptions.Enabled.Should().BeTrue();
        sessionOptions.Enabled.Should().BeTrue();
        weavingOptions.DefaultResponseReservedTokens.Should().Be(4096);
    }

    [Fact]
    // [DL.01] Method_Scenario_Expected naming convention
    public void AddVKAIPsycheBlock_WithConfiguration_ShouldBindConfigurationSections()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddVKBlockMarker<VKCoreBlock>();
        services.AddVKBlockMarker<VKAIBlock>();

        var configData = new Dictionary<string, string?>
        {
            ["VKBlocks:AIPsyche:Echo:TokenBudgetRatio"] = "0.75",
            ["VKBlocks:AIPsyche:Echo:AutoSaveHistory"] = "false"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act
        services.AddVKAIPsycheBlock(configuration)
            .AddVKEcho();

        using var serviceProvider = services.BuildServiceProvider();
        var echoOptions = serviceProvider.GetRequiredService<IOptions<VKEchoOptions>>().Value;

        // Assert
        echoOptions.TokenBudgetRatio.Should().Be(0.75);
        echoOptions.AutoSaveHistory.Should().BeFalse();
    }
}
