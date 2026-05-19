using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace VK.Blocks.Core.UnitTests.DependencyInjection;

public sealed class VKBlockRegistrationExtensionsTests
{
    private readonly ServiceCollection _services = new ServiceCollection();

    [Fact]
    public void AddVKBlockOptions_FromConfiguration_ShouldBindAndRegister()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Test:Value"] = "Hello"
            })
            .Build();

        // Act
        var options = _services.AddVKBlockOptions<TestOptions>(config);

        // Assert
        options.Value.Should().Be("Hello");

        var provider = _services.BuildServiceProvider();
        provider.GetRequiredService<IOptions<TestOptions>>().Value.Value.Should().Be("Hello");
        provider.GetRequiredService<TestOptions>().Value.Should().Be("Hello");
    }

    [Fact]
    public void AddVKBlockOptions_FromFunc_ShouldConfigureAndRegister()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();

        // Act
        var options = _services.AddVKBlockOptions<TestOptions>(config, o => o with { Value = "Manual" });

        // Assert
        options.Value.Should().Be("Manual");

        var provider = _services.BuildServiceProvider();
        provider.GetRequiredService<IOptions<TestOptions>>().Value.Value.Should().Be("Manual");
        provider.GetRequiredService<TestOptions>().Value.Should().Be("Manual");
    }

    [Fact]
    public void AddVKBlockOptions_IsIdempotent()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();

        // Act
        _services.AddVKBlockOptions<TestOptions>(config);
        var countBefore = _services.Count;
        _services.AddVKBlockOptions<TestOptions>(config);

        // Assert
        _services.Count.Should().Be(countBefore);
    }

    [Fact]
    public void WithScoped_ShouldReplaceRegistration()
    {
        // Arrange
        _services.AddScoped<ITestService, DefaultService>();
        var config = new ConfigurationBuilder().Build();
        var builder = new VKBlockBuilder<TestMarker>(_services, config);

        // Act
        builder.WithScoped<TestMarker, ITestService, CustomService>();

        // Assert
        var descriptor = _services.Single(d => d.ServiceType == typeof(ITestService));
        descriptor.ImplementationType.Should().Be(typeof(CustomService));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void WithSingleton_ShouldReplaceRegistration()
    {
        // Arrange
        _services.AddSingleton<ITestService, DefaultService>();
        var config = new ConfigurationBuilder().Build();
        var builder = new VKBlockBuilder<TestMarker>(_services, config);

        // Act
        builder.WithSingleton<TestMarker, ITestService, CustomService>();

        // Assert
        var descriptor = _services.Single(d => d.ServiceType == typeof(ITestService));
        descriptor.ImplementationType.Should().Be(typeof(CustomService));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void WithTransient_ShouldReplaceRegistration()
    {
        // Arrange
        _services.AddTransient<ITestService, DefaultService>();
        var config = new ConfigurationBuilder().Build();
        var builder = new VKBlockBuilder<TestMarker>(_services, config);

        // Act
        builder.WithTransient<TestMarker, ITestService, CustomService>();

        // Assert
        var descriptor = _services.Single(d => d.ServiceType == typeof(ITestService));
        descriptor.ImplementationType.Should().Be(typeof(CustomService));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Transient);
    }

    [Fact]
    public void TryAddEnumerableScoped_ShouldAddOnlyOnce()
    {
        // Act
        _services.TryAddEnumerableScoped<ITestService, CustomService>();
        _services.TryAddEnumerableScoped<ITestService, CustomService>();

        // Assert
        _services.Where(d => d.ServiceType == typeof(ITestService)).Should().HaveCount(1);
    }

    [Fact]
    public void Builder_TryAddEnumerableScoped_ShouldAddOnlyOnce()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();
        var builder = new VKBlockBuilder<TestMarker>(_services, config);

        // Act
        builder.TryAddEnumerableScoped<TestMarker, ITestService, CustomService>();
        builder.TryAddEnumerableScoped<TestMarker, ITestService, CustomService>();

        // Assert
        _services.Where(d => d.ServiceType == typeof(ITestService)).Should().HaveCount(1);
    }

    [Fact]
    public void TryAddEnumerableSingleton_ShouldAddOnlyOnce()
    {
        // Act
        _services.TryAddEnumerableSingleton<ITestService, CustomService>();
        _services.TryAddEnumerableSingleton<ITestService, CustomService>();

        // Assert
        _services.Where(d => d.ServiceType == typeof(ITestService)).Should().HaveCount(1);
        _services.First(d => d.ServiceType == typeof(ITestService)).Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void TryAddEnumerableTransient_ShouldAddOnlyOnce()
    {
        // Act
        _services.TryAddEnumerableTransient<ITestService, CustomService>();
        _services.TryAddEnumerableTransient<ITestService, CustomService>();

        // Assert
        _services.Where(d => d.ServiceType == typeof(ITestService)).Should().HaveCount(1);
        _services.First(d => d.ServiceType == typeof(ITestService)).Lifetime.Should().Be(ServiceLifetime.Transient);
    }

    [Fact]
    public void Builder_TryAddEnumerableSingleton_ShouldAddOnlyOnce()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();
        var builder = new VKBlockBuilder<TestMarker>(_services, config);

        // Act
        builder.TryAddEnumerableSingleton<TestMarker, ITestService, CustomService>();
        builder.TryAddEnumerableSingleton<TestMarker, ITestService, CustomService>();

        // Assert
        _services.Where(d => d.ServiceType == typeof(ITestService)).Should().HaveCount(1);
    }

    [Fact]
    public void Builder_TryAddEnumerableTransient_ShouldAddOnlyOnce()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();
        var builder = new VKBlockBuilder<TestMarker>(_services, config);

        // Act
        builder.TryAddEnumerableTransient<TestMarker, ITestService, CustomService>();
        builder.TryAddEnumerableTransient<TestMarker, ITestService, CustomService>();

        // Assert
        _services.Where(d => d.ServiceType == typeof(ITestService)).Should().HaveCount(1);
    }

    private sealed record TestOptions : IVKBlockOptions
    {
        public static string SectionName => "Test";
        public string Value { get; init; } = "";
    }

    private interface ITestService;
    private sealed class DefaultService : ITestService;
    private sealed class CustomService : ITestService;
    private sealed class TestMarker : IVKBlockMarker, IVKBlockMarkerProvider<TestMarker>
    {
        public static IVKBlockMarker Instance { get; } = new TestMarker();
        public string Name => "Test";
        public string Identifier => "Test";
        public string Version => "1.0.0";
        public IReadOnlyList<IVKBlockMarker> Dependencies => [];
        public string ActivitySourceName => "Test";
        public string MeterName => "Test";
    }

    [Fact]
    public void EnsureCoreBlockRegistered_WhenMissing_ShouldThrow()
    {
        // Act
        Action act = () => _services.EnsureCoreBlockRegistered<TestMarker>();

        // Assert
        act.Should().Throw<VKDependencyException>()
            .WithMessage("*requires 'VK.Blocks.Core' to be registered first*");
    }

    [Fact]
    public void EnsureCoreBlockRegistered_WhenPresent_ShouldNotThrow()
    {
        // Arrange
        _services.AddVKCoreBlock(new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build());

        // Act
        Action act = () => _services.EnsureCoreBlockRegistered<TestMarker>();

        // Assert
        act.Should().NotThrow();
    }

    // ==========================================
    // OPTIONS REGISTRATION TESTS (A-F Scenarios)
    // ==========================================

    #region A. Configuration Binding

    // A-01: Section exists, binds successfully
    [Fact]
    public void AddVKBlockOptions_WhenSectionExists_BindsOptionsAndRegistersSingletons()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Validated:RequiredValue"] = "Exists",
                ["Validated:NumberValue"] = "42"
            })
            .Build();

        // Act
        var result = _services.AddVKBlockOptions<ValidatedOptions>(config);
        using var provider = _services.BuildServiceProvider();

        // Assert
        result.RequiredValue.Should().Be("Exists");
        result.NumberValue.Should().Be(42);

        provider.GetRequiredService<ValidatedOptions>().Should().BeSameAs(result);
        provider.GetRequiredService<IOptions<ValidatedOptions>>().Value.Should().BeSameAs(result);
    }

    // A-02: Section does not exist at all
    // If there is validation for required fields, an exception is thrown on first access
    [Fact]
    public void AddVKBlockOptions_WhenSectionDoesNotExistAndHasRequiredFields_ThrowsOptionsValidationException()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>()) // Empty config
            .Build();

        // Act
        _services.AddVKBlockOptions<ValidatedOptions>(config);
        using var provider = _services.BuildServiceProvider();

        // Assert
        var act = () => _ = provider.GetRequiredService<IOptions<ValidatedOptions>>().Value;
        act.Should().Throw<OptionsValidationException>()
            .And.Message.Should().Contain("RequiredValue");
    }

    // A-03: Section exists but all values are null
    [Fact]
    public void AddVKBlockOptions_WhenSectionIsEmptyAndHasRequiredFields_ThrowsOptionsValidationException()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Validated:RequiredValue"] = null,
                ["Validated:NumberValue"] = null
            })
            .Build();

        // Act
        _services.AddVKBlockOptions<ValidatedOptions>(config);
        using var provider = _services.BuildServiceProvider();

        // Assert
        var act = () => _ = provider.GetRequiredService<IOptions<ValidatedOptions>>().Value;
        act.Should().Throw<OptionsValidationException>();
    }

    // A-04: Some fields are missing in the section (required properties are not set)
    [Fact]
    public void AddVKBlockOptions_WhenRequiredFieldsMissing_ThrowsOptionsValidationException()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Validated:NumberValue"] = "10" // RequiredValue is missing
            })
            .Build();

        // Act
        _services.AddVKBlockOptions<ValidatedOptions>(config);
        using var provider = _services.BuildServiceProvider();

        // Assert
        var act = () => _ = provider.GetRequiredService<IOptions<ValidatedOptions>>().Value;
        act.Should().Throw<OptionsValidationException>()
            .And.Message.Should().Contain("RequiredValue");
    }

    #endregion

    #region B. Transform Chain

    // B-01: transform is null, no transformation is performed
    [Fact]
    public void AddVKBlockOptions_WhenTransformIsNull_KeepsOriginalConfigValues()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Validated:RequiredValue"] = "Original",
                ["Validated:NumberValue"] = "42"
            })
            .Build();

        // Act
        var result = _services.AddVKBlockOptions<ValidatedOptions>(config, transform: null);

        // Assert
        result.RequiredValue.Should().Be("Original");
    }

    // B-02: transform modifies a single field
    [Fact]
    public void AddVKBlockOptions_WhenTransformModifiesField_ReturnsTransformedInstance()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Validated:RequiredValue"] = "Original",
                ["Validated:NumberValue"] = "42"
            })
            .Build();

        // Act
        var result = _services.AddVKBlockOptions<ValidatedOptions>(config, opt => opt with { RequiredValue = "Overridden" });
        using var provider = _services.BuildServiceProvider();

        // Assert
        result.RequiredValue.Should().Be("Overridden");
        provider.GetRequiredService<ValidatedOptions>().RequiredValue.Should().Be("Overridden");
        provider.GetRequiredService<IOptions<ValidatedOptions>>().Value.RequiredValue.Should().Be("Overridden");
    }

    // B-03: transform returns the same instance (identity transform)
    [Fact]
    public void AddVKBlockOptions_WhenTransformReturnsSameInstance_SucceedsNormally()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Validated:RequiredValue"] = "Original"
            })
            .Build();

        // Act
        var result = _services.AddVKBlockOptions<ValidatedOptions>(config, opt => opt);

        // Assert
        result.RequiredValue.Should().Be("Original");
    }

    // B-04: transform changes a required field to an empty string
    [Fact]
    public void AddVKBlockOptions_WhenTransformCreatesInvalidState_ValidationCatchesItAndThrows()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Validated:RequiredValue"] = "ValidOriginal"
            })
            .Build();

        // Act
        _services.AddVKBlockOptions<ValidatedOptions>(config, opt => opt with { RequiredValue = "" });
        using var provider = _services.BuildServiceProvider();

        // Assert
        var act = () => _ = provider.GetRequiredService<IOptions<ValidatedOptions>>().Value;
        act.Should().Throw<OptionsValidationException>();
    }

    // B-05: transform throws an exception
    [Fact]
    public void AddVKBlockOptions_WhenTransformThrowsException_PropagatesException()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Validated:RequiredValue"] = "Original"
            })
            .Build();

        // Act
        Action act = () => _services.AddVKBlockOptions<ValidatedOptions>(config, opt => throw new InvalidOperationException("boom"));

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("boom");
    }

    #endregion

    #region C. Validation Rules

    // C-01: DataAnnotations validation fails
    [Fact]
    public void AddVKBlockOptions_WhenDataAnnotationsViolated_ThrowsValidationException()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Validated:RequiredValue"] = "" // violates [Required]
            })
            .Build();

        // Act
        _services.AddVKBlockOptions<ValidatedOptions>(config);
        using var provider = _services.BuildServiceProvider();

        // Assert
        var act = () => _ = provider.GetRequiredService<IOptions<ValidatedOptions>>().Value;
        act.Should().Throw<OptionsValidationException>();
    }

    // C-02: Custom IValidateOptions validation fails
    [Fact]
    public void AddVKBlockOptions_WhenCustomValidatorFails_ThrowsValidationException()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Validated:RequiredValue"] = "Valid",
                ["Validated:NumberValue"] = "-5" // custom validator should fail
            })
            .Build();

        _services.AddVKBlockOptions<ValidatedOptions>(config);
        _services.AddSingleton<IValidateOptions<ValidatedOptions>, CustomValidatedOptionsValidator>();

        using var provider = _services.BuildServiceProvider();

        // Act & Assert
        var act = () => _ = provider.GetRequiredService<IOptions<ValidatedOptions>>().Value;
        act.Should().Throw<OptionsValidationException>().And.Message.Should().Contain("NumberValue must be positive.");
    }

    // C-03: Multiple validators, all fail
    [Fact]
    public void AddVKBlockOptions_WhenMultipleValidatorsFail_DoesNotShortCircuitAndCollectsAllFailures()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Validated:RequiredValue"] = "Valid",
                ["Validated:NumberValue"] = "-10"
            })
            .Build();

        _services.AddVKBlockOptions<ValidatedOptions>(config);
        _services.AddSingleton<IValidateOptions<ValidatedOptions>, CustomValidatedOptionsValidator>();
        _services.AddSingleton<IValidateOptions<ValidatedOptions>>(new MockFailureValidator("Mock error 1"));
        _services.AddSingleton<IValidateOptions<ValidatedOptions>>(new MockFailureValidator("Mock error 2"));

        using var provider = _services.BuildServiceProvider();

        // Act
        var act = () => _ = provider.GetRequiredService<IOptions<ValidatedOptions>>().Value;

        // Assert
        var ex = act.Should().Throw<OptionsValidationException>().Which;
        ex.Failures.Should().HaveCount(3);
        ex.Failures.Should().Contain("NumberValue must be positive.");
        ex.Failures.Should().Contain("Mock error 1");
        ex.Failures.Should().Contain("Mock error 2");
    }

    // C-04: Multiple validators, some fail
    [Fact]
    public void AddVKBlockOptions_WhenSomeValidatorsFail_CollectsOnlyFailingErrors()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Validated:RequiredValue"] = "Valid",
                ["Validated:NumberValue"] = "10" // CustomValidatedOptionsValidator succeeds
            })
            .Build();

        _services.AddVKBlockOptions<ValidatedOptions>(config);
        _services.AddSingleton<IValidateOptions<ValidatedOptions>, CustomValidatedOptionsValidator>();
        _services.AddSingleton<IValidateOptions<ValidatedOptions>>(new MockFailureValidator("Mock error 1"));
        _services.AddSingleton<IValidateOptions<ValidatedOptions>>(new MockSuccessValidator());
        _services.AddSingleton<IValidateOptions<ValidatedOptions>>(new MockFailureValidator("Mock error 3"));

        using var provider = _services.BuildServiceProvider();

        // Act
        var act = () => _ = provider.GetRequiredService<IOptions<ValidatedOptions>>().Value;

        // Assert
        var ex = act.Should().Throw<OptionsValidationException>().Which;
        ex.Failures.Should().HaveCount(2);
        ex.Failures.Should().Contain("Mock error 1");
        ex.Failures.Should().Contain("Mock error 3");
    }

    // C-05: All validators pass
    [Fact]
    public void AddVKBlockOptions_WhenAllValidatorsSucceed_ResolvesSuccessfully()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Validated:RequiredValue"] = "Valid",
                ["Validated:NumberValue"] = "5"
            })
            .Build();

        _services.AddVKBlockOptions<ValidatedOptions>(config);
        _services.AddSingleton<IValidateOptions<ValidatedOptions>, CustomValidatedOptionsValidator>();
        _services.AddSingleton<IValidateOptions<ValidatedOptions>>(new MockSuccessValidator());

        using var provider = _services.BuildServiceProvider();

        // Act & Assert
        var act = () => _ = provider.GetRequiredService<IOptions<ValidatedOptions>>().Value;
        act.Should().NotThrow();
    }

    #endregion

    #region D. Multiple registrations of the same type and idempotency

    // D-01: Same TOptions registered twice without transform
    [Fact]
    public void AddVKBlockOptions_WhenCalledTwiceWithoutTransform_ReturnsSameInstanceAndSingleRegistration()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Validated:RequiredValue"] = "Original"
            })
            .Build();

        // Act
        var first = _services.AddVKBlockOptions<ValidatedOptions>(config);
        var second = _services.AddVKBlockOptions<ValidatedOptions>(config);

        // Assert
        first.Should().BeSameAs(second);
        _services.Count(d => d.ServiceType == typeof(ValidatedOptions)).Should().Be(1);
    }

    // D-02: Same TOptions registered twice, second one with transform
    [Fact]
    public void AddVKBlockOptions_WhenCalledTwiceSecondWithTransform_ReplacesSingletonAndFactory()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Validated:RequiredValue"] = "Original"
            })
            .Build();

        // Act
        var first = _services.AddVKBlockOptions<ValidatedOptions>(config);
        var second = _services.AddVKBlockOptions<ValidatedOptions>(config, opt => opt with { RequiredValue = "Modified" });
        using var provider = _services.BuildServiceProvider();

        // Assert
        first.RequiredValue.Should().Be("Original");
        second.RequiredValue.Should().Be("Modified");

        provider.GetRequiredService<ValidatedOptions>().RequiredValue.Should().Be("Modified");
        provider.GetRequiredService<IOptions<ValidatedOptions>>().Value.RequiredValue.Should().Be("Modified");
    }

    // D-03: Same TOptions registered sequentially multiple times, verifying that chained transforms apply cumulatively, and the final instance reflects the state of the last registration
    [Fact]
    public void AddVKBlockOptions_WhenCalledThriceWithTransforms_AppliesCumulativeTransforms()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Validated:RequiredValue"] = "Original",
                ["Validated:NumberValue"] = "1"
            })
            .Build();

        // Act
        _services.AddVKBlockOptions<ValidatedOptions>(config);
        _services.AddVKBlockOptions<ValidatedOptions>(config, opt => opt with { RequiredValue = "Step2" });
        _services.AddVKBlockOptions<ValidatedOptions>(config, opt => opt with { NumberValue = 100 });
        using var provider = _services.BuildServiceProvider();

        // Assert
        var resolved = provider.GetRequiredService<ValidatedOptions>();
        resolved.RequiredValue.Should().Be("Step2");
        resolved.NumberValue.Should().Be(100);
    }

    // D-04: Different TOptions types each registered once
    [Fact]
    public void AddVKBlockOptions_WhenDifferentTypesRegistered_DoNotInterfere()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Test:Value"] = "TestVal",
                ["Validated:RequiredValue"] = "ValVal"
            })
            .Build();

        // Act
        var testResult = _services.AddVKBlockOptions<TestOptions>(config);
        var validatedResult = _services.AddVKBlockOptions<ValidatedOptions>(config);

        // Assert
        testResult.Value.Should().Be("TestVal");
        validatedResult.RequiredValue.Should().Be("ValVal");
    }

    #endregion

    #region E. DI Lifecycle and Resolve Patterns

    // E-01: Direct injection of TOptions works
    [Fact]
    public void AddVKBlockOptions_ShouldAllowDirectInjectionOfTOptions()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Validated:RequiredValue"] = "Exists"
            })
            .Build();

        // Act
        _services.AddVKBlockOptions<ValidatedOptions>(config);
        using var provider = _services.BuildServiceProvider();

        // Assert
        var resolved = provider.GetRequiredService<ValidatedOptions>();
        resolved.RequiredValue.Should().Be("Exists");
    }

    // E-02: IOptions<TOptions>.Value works
    [Fact]
    public void AddVKBlockOptions_ShouldAllowIOptionsInjection()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Validated:RequiredValue"] = "Exists"
            })
            .Build();

        // Act
        var bound = _services.AddVKBlockOptions<ValidatedOptions>(config);
        using var provider = _services.BuildServiceProvider();

        // Assert
        var resolved = provider.GetRequiredService<IOptions<ValidatedOptions>>().Value;
        resolved.Should().BeSameAs(bound);
    }

    // E-03: IOptionsMonitor<TOptions>.CurrentValue works
    [Fact]
    public void AddVKBlockOptions_ShouldAllowIOptionsMonitorInjection()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Validated:RequiredValue"] = "Exists"
            })
            .Build();

        // Act
        var bound = _services.AddVKBlockOptions<ValidatedOptions>(config);
        using var provider = _services.BuildServiceProvider();

        // Assert
        var resolved = provider.GetRequiredService<IOptionsMonitor<ValidatedOptions>>().CurrentValue;
        resolved.Should().BeSameAs(bound);
    }

    // E-04: IOptionsSnapshot<TOptions>.Value works
    [Fact]
    public void AddVKBlockOptions_ShouldAllowIOptionsSnapshotInjectionWithinScope()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Validated:RequiredValue"] = "Exists"
            })
            .Build();

        // Act
        var bound = _services.AddVKBlockOptions<ValidatedOptions>(config);
        using var provider = _services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        // Assert
        var resolved = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<ValidatedOptions>>().Value;
        resolved.Should().BeSameAs(bound);
    }

    // E-05: Registered TOptions singleton resolved twice returns the exact same instance (not reconstructed)
    [Fact]
    public void AddVKBlockOptions_ShouldReturnSameSingletonInstanceOnMultipleResolutions()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Validated:RequiredValue"] = "Exists"
            })
            .Build();

        // Act
        _services.AddVKBlockOptions<ValidatedOptions>(config);
        using var provider = _services.BuildServiceProvider();

        var instance1 = provider.GetRequiredService<ValidatedOptions>();
        var instance2 = provider.GetRequiredService<ValidatedOptions>();

        // Assert
        instance1.Should().BeSameAs(instance2);
    }

    #endregion

    #region F. Boundary & Guard Conditions

    // F-01: services = null
    [Fact]
    public void AddVKBlockOptions_WhenServicesIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection nullServices = null!;
        var config = new ConfigurationBuilder().Build();

        // Act
        Action act = () => nullServices.AddVKBlockOptions<ValidatedOptions>(config);

        // Assert
        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("services");
    }

    // F-02: configuration = null
    [Fact]
    public void AddVKBlockOptions_WhenConfigurationIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        IConfiguration nullConfig = null!;

        // Act
        Action act = () => _services.AddVKBlockOptions<ValidatedOptions>(nullConfig);

        // Assert
        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("configuration");
    }

    // F-03: SectionName returns an empty string or whitespace
    // An empty SectionName is a developer implementation error, and the framework should throw an ArgumentException at the entry point using VKGuard.
    [Fact]
    public void AddVKBlockOptions_WhenSectionNameIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();

        // Act
        Action act = () => _services.AddVKBlockOptions<EmptySectionOptions>(config);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Options SectionName cannot be null or empty.*");
    }

    // F-04: Mismatched configuration value types
    // In modern .NET Configuration, Get<T>() throws an InvalidOperationException during the binding phase when encountering an unconvertible data type (e.g., converting "not-a-number" to an int). If the configuration framework does not throw, it will eventually be intercepted by the validation chain.
    [Fact]
    public void AddVKBlockOptions_WhenConfigurationValueTypeMismatched_ThrowsException()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Validated:RequiredValue"] = "Valid",
                ["Validated:NumberValue"] = "not-a-number" // Violates the int data type constraint
            })
            .Build();

        // Act
        Action act = () => _services.AddVKBlockOptions<ValidatedOptions>(config);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    // F-05: transform is null and section does not exist
    [Fact]
    public void AddVKBlockOptions_WhenTransformIsNullAndSectionDoesNotExist_ThrowsValidationException()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();

        // Act
        _services.AddVKBlockOptions<ValidatedOptions>(config, transform: null);
        using var provider = _services.BuildServiceProvider();

        // Assert
        var act = () => _ = provider.GetRequiredService<IOptions<ValidatedOptions>>().Value;
        act.Should().Throw<OptionsValidationException>();
    }

    #endregion

    #region G. Startup Integration Tests

    // G-01: ValidateOnStart is triggered during Host startup
    // Verifies that options configured with ValidateOnStart() immediately throw an OptionsValidationException during application startup when building and starting the generic Host (IHost), rather than waiting for the first business consumption.
    [Fact]
    public async Task AddVKBlockOptions_ValidateOnStart_TriggersValidationOnHostStartup()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Validated:RequiredValue"] = "" // Violates the [Required] field constraint
            })
            .Build();

        var host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddVKBlockOptions<ValidatedOptions>(config);
            })
            .Build();

        // Act
        Func<Task> act = () => host.StartAsync();

        // Assert
        await act.Should().ThrowAsync<OptionsValidationException>();
    }

    #endregion

    #region H. Core Immutable & Injection Contracts

    // H-01: sealed record + required + init properties can be parsed and bound successfully
    [Fact]
    public void AddVKBlockOptions_WhenUsingSealedRecordWithRequiredInit_BindsSuccessfully()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RequiredInit:RequiredValue"] = "ImmutableValue",
                ["RequiredInit:NumberValue"] = "99"
            })
            .Build();

        // Act
        var result = _services.AddVKBlockOptions<RequiredInitOptions>(config);
        using var provider = _services.BuildServiceProvider();

        // Assert
        result.RequiredValue.Should().Be("ImmutableValue");
        result.NumberValue.Should().Be(99);

        var resolved = provider.GetRequiredService<RequiredInitOptions>();
        resolved.RequiredValue.Should().Be("ImmutableValue");
        resolved.NumberValue.Should().Be(99);
    }

    // H-02: Three main resolution patterns return the exact same singleton reference (direct injection, IOptions<T>, and IOptionsMonitor<T>)
    [Fact]
    public void AddVKBlockOptions_ThreeResolutionPatterns_ReturnExactSameInstance()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Validated:RequiredValue"] = "ReferenceChecking"
            })
            .Build();

        // Act
        var original = _services.AddVKBlockOptions<ValidatedOptions>(config);
        using var provider = _services.BuildServiceProvider();

        var direct = provider.GetRequiredService<ValidatedOptions>();
        var viaOpts = provider.GetRequiredService<IOptions<ValidatedOptions>>().Value;
        var viaMonitor = provider.GetRequiredService<IOptionsMonitor<ValidatedOptions>>().CurrentValue;

        // Assert
        direct.Should().BeSameAs(original);
        viaOpts.Should().BeSameAs(original);
        viaMonitor.Should().BeSameAs(original);
    }

    #endregion

    #region Helper Classes & Mocks

    private sealed record ValidatedOptions : IVKBlockOptions
    {
        public static string SectionName => "Validated";

        [System.ComponentModel.DataAnnotations.Required]
        public string RequiredValue { get; init; } = "";

        public int NumberValue { get; init; } = 0;
    }

    private sealed record RequiredInitOptions : IVKBlockOptions
    {
        public static string SectionName => "RequiredInit";

        [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
        public RequiredInitOptions()
        {
        }

        public required string RequiredValue { get; init; } = "";
        public required int NumberValue { get; init; } = 0;
    }

    private sealed class CustomValidatedOptionsValidator : IValidateOptions<ValidatedOptions>
    {
        public ValidateOptionsResult Validate(string? name, ValidatedOptions options)
        {
            if (options.NumberValue < 0)
            {
                return ValidateOptionsResult.Fail("NumberValue must be positive.");
            }
            return ValidateOptionsResult.Success;
        }
    }

    private sealed record EmptySectionOptions : IVKBlockOptions
    {
        public static string SectionName => ""; // Empty Section

        [System.ComponentModel.DataAnnotations.Required]
        public string RequiredValue { get; init; } = "";
    }

    private sealed class MockFailureValidator : IValidateOptions<ValidatedOptions>
    {
        private readonly string _error;
        public MockFailureValidator(string error) => _error = error;

        public ValidateOptionsResult Validate(string? name, ValidatedOptions options)
        {
            return ValidateOptionsResult.Fail(_error);
        }
    }

    private sealed class MockSuccessValidator : IValidateOptions<ValidatedOptions>
    {
        public ValidateOptionsResult Validate(string? name, ValidatedOptions options)
        {
            return ValidateOptionsResult.Success;
        }
    }

    #endregion
}
