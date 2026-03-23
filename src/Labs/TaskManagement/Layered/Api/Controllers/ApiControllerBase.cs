using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using VK.Blocks.Core.Results;
using VK.Blocks.ExceptionHandling.Abstractions.Contracts;
using VK.Blocks.Web.Presentation;

namespace VK.Labs.TaskManagement.Layered.Api.Controllers;

// We can now just inherit from VKApiController and automatically get HandleResult
// and standard mapping to VKProblemDetails in the background.
public abstract class ApiControllerBase : VKApiController
{
}
