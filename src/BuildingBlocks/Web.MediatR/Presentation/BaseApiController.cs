using MediatR;
using Microsoft.AspNetCore.Mvc;
using VK.Blocks.Web.Presentation;

namespace VK.Blocks.Web.MediatR.Presentation;

/// <summary>
/// Base controller for CQRS API controllers, extending <see cref="VKApiController"/> by injecting MediatR's ISender.
/// </summary>
public abstract class BaseApiController(ISender sender) : VKApiController
{
    #region Properties

    /// <summary>
    /// Gets the MediatR sender.
    /// </summary>
    protected ISender Sender => sender;

    #endregion
}
