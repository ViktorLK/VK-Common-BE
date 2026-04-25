using System.Diagnostics;

namespace VK.Blocks.Core.UnitTests.Utilities;

public class StopwatchExtensionsTests
{
    private static readonly ActivitySource Source = new("Test.Source");

    [Fact]
    public void RecordProcess_WhenActivityExists_ShouldSetTags()
    {
        // Arrange
        using var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStarted = _ => { },
            ActivityStopped = _ => { }
        };
        ActivitySource.AddActivityListener(listener);

        var sw = Stopwatch.StartNew();
        var actionName = "TestAction";
        var result = VKResult.Success();

        using (var activity = Source.StartActivity("TestActivity"))
        {
            // Act
            sw.RecordProcess(actionName, result);

            // Assert
            activity.Should().NotBeNull();
            activity!.TagObjects.Should().Contain(t => t.Key == "vk.process.name" && t.Value!.ToString() == actionName);
            activity.TagObjects.Should().Contain(t => t.Key == "vk.process.success" && (bool)t.Value! == true);
            activity.TagObjects.Should().Contain(t => t.Key == "vk.process.duration_ms");
        }
    }

    [Fact]
    public void RecordProcess_WhenProcessFails_ShouldSetErrorTags()
    {
        // Arrange
        using var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(listener);

        var sw = Stopwatch.StartNew();
        var error = VKError.Validation("Err.01", "Invalid data");
        var result = VKResult.Failure(error);

        using (var activity = Source.StartActivity("TestActivity"))
        {
            // Act
            sw.RecordProcess("FailedAction", result);

            // Assert
            activity.Should().NotBeNull();
            activity!.TagObjects.Should().Contain(t => t.Key == "vk.process.success" && (bool)t.Value! == false);
            activity.TagObjects.Should().Contain(t => t.Key == "vk.process.error_code" && t.Value!.ToString() == error.Code);
            activity.TagObjects.Should().Contain(t => t.Key == "vk.process.error_type" && t.Value!.ToString() == error.Type.ToString());
        }
    }

    [Fact]
    public void RecordProcess_WhenNoActivityExists_ShouldNotThrow()
    {
        // Arrange
        var sw = Stopwatch.StartNew();
        var result = VKResult.Success();

        // Act
        Action act = () => sw.RecordProcess("NoActivityAction", result);

        // Assert
        act.Should().NotThrow();
    }
}
