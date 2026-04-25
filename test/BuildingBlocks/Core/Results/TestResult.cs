namespace VK.Blocks.Core.UnitTests.Results;

public class TestResult : VKResult
{
    public TestResult(bool isSuccess, VKError error) : base(isSuccess, error) { }
    public TestResult(bool isSuccess, IEnumerable<VKError> errors) : base(isSuccess, errors) { }
}
