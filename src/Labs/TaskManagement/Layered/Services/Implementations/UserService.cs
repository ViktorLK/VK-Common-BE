using VK.Labs.TaskManagement.Layered.Data.Entities;
using VK.Labs.TaskManagement.Layered.Data.Repositories.Interfaces;
using VK.Labs.TaskManagement.Layered.Services.Interfaces;

namespace VK.Labs.TaskManagement.Layered.Services.Implementations;

public sealed class UserService(IUserRepository userRepository) : IUserService
{
    public Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return userRepository.GetByIdAsync(id, cancellationToken);
    }

    public Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return userRepository.GetByEmailAsync(email, cancellationToken);
    }
}
