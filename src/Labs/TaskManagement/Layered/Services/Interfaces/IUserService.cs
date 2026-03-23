using VK.Labs.TaskManagement.Layered.Data.Entities;

namespace VK.Labs.TaskManagement.Layered.Services.Interfaces;

public interface IUserService
{
    Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
}
