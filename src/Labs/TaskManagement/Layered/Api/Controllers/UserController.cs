using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VK.Labs.TaskManagement.Layered.Services.Interfaces;

namespace VK.Labs.TaskManagement.Layered.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class UserController(IUserService userService) : ApiControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var user = await userService.GetUserByIdAsync(id, cancellationToken);
        if (user == null) return NotFound();

        // Should return a standard response object without sensitive info (like password) directly from entities
        return Ok(new { user.Id, user.Username, user.Email });
    }
}
