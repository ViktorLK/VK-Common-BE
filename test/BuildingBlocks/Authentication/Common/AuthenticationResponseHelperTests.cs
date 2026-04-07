using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VK.Blocks.Authentication.Common;
using Xunit;

namespace VK.Blocks.Authentication.UnitTests.Common;

public sealed class AuthenticationResponseHelperTests
{
    [Fact]
    public async Task WriteUnauthorizedResponseAsync_ShouldSetStandardizedProperties()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var responseStream = new MemoryStream();
        context.Response.Body = responseStream;
        context.TraceIdentifier = "test-trace-id";
        context.Request.Path = "/api/test";
        var detail = "Access Denied";

        // Act
        await AuthenticationResponseHelper.WriteUnauthorizedResponseAsync(context, detail);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
        context.Response.ContentType.Should().StartWith("application/problem+json");

        responseStream.Position = 0;
        var problemDetails = await JsonSerializer.DeserializeAsync<ProblemDetails>(responseStream);

        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be((int)HttpStatusCode.Unauthorized);
        problemDetails.Title.Should().Be(AuthenticationConstants.UnauthorizedTitle);
        problemDetails.Detail.Should().Be(detail);
        problemDetails.Instance.Should().Be("/api/test");
        
        problemDetails!.Extensions.Should().ContainKey(AuthenticationConstants.TraceIdExtension)
            .WhoseValue.ToString().Should().Be("test-trace-id");
    }
}
