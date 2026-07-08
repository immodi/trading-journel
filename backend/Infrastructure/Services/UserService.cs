using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Mappers;
using Application.Requests.User;

namespace Infrastructure.Services;

public class UserService(IUserRepository repository, IPasswordHasher passwordHasher) : IUserService
{
    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = await repository.GetAllAsync();
        return users.Select(t => t.ToDto());
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await repository.GetByIdAsync(id);
        var dto = user?.ToDto();
        return dto;
    }

    public async Task AddAsync(CreateUserRequest request)
    {
        var user = request.ToEntity(passwordHasher);
        await repository.AddAsync(user);
        await repository.SaveChangesAsync();
    }

    public async Task UpdateAsync(int id, UpdateUserRequest request)
    {
        var user = await repository.GetByIdAsync(id);

        if (user is null)
            throw new KeyNotFoundException($"User with id {id} was not found.");

        request.UpdateUser(user, passwordHasher);

        await repository.UpdateAsync(user);
        await repository.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await repository.DeleteAsync(id);
        await repository.SaveChangesAsync();
    }
}