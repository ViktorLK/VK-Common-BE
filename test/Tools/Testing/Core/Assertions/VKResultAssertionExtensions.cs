using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using VK.Blocks.Core;

namespace VK.Blocks.Testing;

/// <summary>
/// FluentAssertions extension methods for <see cref="VKResult"/> and <see cref="VKResult{TValue}"/>.
/// </summary>
public static class VKResultAssertionExtensions
{
    /// <summary>
    /// Returns a <see cref="VKResultAssertions"/> object that can be used to assert the current <see cref="VKResult"/>.
    /// </summary>
    public static VKResultAssertions Should(this VKResult result) => new(result);

    /// <summary>
    /// Returns a <see cref="VKResultAssertions{TValue}"/> object that can be used to assert the current <see cref="VKResult{TValue}"/>.
    /// </summary>
    public static VKResultAssertions<TValue> Should<TValue>(this VKResult<TValue> result) => new(result);
}

/// <summary>
/// Provides assertion methods for <see cref="VKResult"/>.
/// </summary>
public class VKResultAssertions : ObjectAssertions<VKResult, VKResultAssertions>
{
    public VKResultAssertions(VKResult subject) : base(subject, AssertionChain.GetOrCreate())
    {
    }

    /// <summary>
    /// Asserts that the result indicates success.
    /// </summary>
    public AndConstraint<VKResultAssertions> BeSuccess(string because = "", params object[] becauseArgs)
    {
        CurrentAssertionChain
            .ForCondition(Subject.IsSuccess)
            .BecauseOf(because, becauseArgs)
            .FailWith("Expected result to be successful{reason}, but it failed with errors: {0}.",
                Subject.Errors is { Length: > 0 } ? string.Join(", ", Subject.Errors.Select(e => $"{e.Code}: {e.Description}")) : "Unknown error");

        return new AndConstraint<VKResultAssertions>(this);
    }

    /// <summary>
    /// Asserts that the result indicates failure.
    /// </summary>
    public AndConstraint<VKResultAssertions> BeFailure(string because = "", params object[] becauseArgs)
    {
        CurrentAssertionChain
            .ForCondition(Subject.IsFailure)
            .BecauseOf(because, becauseArgs)
            .FailWith("Expected result to be a failure{reason}, but it was successful.");

        return new AndConstraint<VKResultAssertions>(this);
    }

    /// <summary>
    /// Asserts that the result indicates failure with the specified error code.
    /// </summary>
    public AndConstraint<VKResultAssertions> BeFailure(string errorCode, string because = "", params object[] becauseArgs)
    {
        BeFailure(because, becauseArgs);

        CurrentAssertionChain
            .ForCondition(Subject.Errors.Any(e => e.Code == errorCode))
            .BecauseOf(because, becauseArgs)
            .FailWith("Expected result to contain error code {0}{reason}, but found: {1}.",
                errorCode,
                string.Join(", ", Subject.Errors.Select(e => e.Code)));

        return new AndConstraint<VKResultAssertions>(this);
    }

    /// <summary>
    /// Asserts that the result indicates failure with the specified <see cref="VKError"/>.
    /// </summary>
    public AndConstraint<VKResultAssertions> BeFailure(VKError expectedError, string because = "", params object[] becauseArgs)
    {
        return BeFailure(expectedError.Code, because, becauseArgs);
    }
}

/// <summary>
/// Provides assertion methods for <see cref="VKResult{TValue}"/>.
/// </summary>
/// <typeparam name="TValue">The type of the encapsulated value.</typeparam>
public class VKResultAssertions<TValue> : ObjectAssertions<VKResult<TValue>, VKResultAssertions<TValue>>
{
    public VKResultAssertions(VKResult<TValue> subject) : base(subject, AssertionChain.GetOrCreate())
    {
    }

    /// <summary>
    /// Asserts that the result indicates success.
    /// </summary>
    public AndConstraint<VKResultAssertions<TValue>> BeSuccess(string because = "", params object[] becauseArgs)
    {
        CurrentAssertionChain
            .ForCondition(Subject.IsSuccess)
            .BecauseOf(because, becauseArgs)
            .FailWith("Expected result to be successful{reason}, but it failed with errors: {0}.",
                Subject.Errors is { Length: > 0 } ? string.Join(", ", Subject.Errors.Select(e => $"{e.Code}: {e.Description}")) : "Unknown error");

        return new AndConstraint<VKResultAssertions<TValue>>(this);
    }

    /// <summary>
    /// Asserts that the result indicates success and has the expected value.
    /// </summary>
    public AndConstraint<VKResultAssertions<TValue>> BeSuccessWithValue(TValue expectedValue, string because = "", params object[] becauseArgs)
    {
        BeSuccess(because, becauseArgs);

        Subject.Value.Should().Be(expectedValue, because, becauseArgs);
        return new AndConstraint<VKResultAssertions<TValue>>(this);
    }

    /// <summary>
    /// Asserts that the result indicates failure.
    /// </summary>
    public AndConstraint<VKResultAssertions<TValue>> BeFailure(string because = "", params object[] becauseArgs)
    {
        CurrentAssertionChain
            .ForCondition(Subject.IsFailure)
            .BecauseOf(because, becauseArgs)
            .FailWith("Expected result to be a failure{reason}, but it was successful.");

        return new AndConstraint<VKResultAssertions<TValue>>(this);
    }

    /// <summary>
    /// Asserts that the result indicates failure with the specified error code.
    /// </summary>
    public AndConstraint<VKResultAssertions<TValue>> BeFailure(string errorCode, string because = "", params object[] becauseArgs)
    {
        BeFailure(because, becauseArgs);

        CurrentAssertionChain
            .ForCondition(Subject.Errors.Any(e => e.Code == errorCode))
            .BecauseOf(because, becauseArgs)
            .FailWith("Expected result to contain error code {0}{reason}, but found: {1}.",
                errorCode,
                string.Join(", ", Subject.Errors.Select(e => e.Code)));

        return new AndConstraint<VKResultAssertions<TValue>>(this);
    }

    /// <summary>
    /// Asserts that the result indicates failure with the specified <see cref="VKError"/>.
    /// </summary>
    public AndConstraint<VKResultAssertions<TValue>> BeFailure(VKError expectedError, string because = "", params object[] becauseArgs)
    {
        return BeFailure(expectedError.Code, because, becauseArgs);
    }
}
