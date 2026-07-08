using Application.DTOs;
using Application.Requests.User;

namespace Application.Interfaces.Services;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(int id);
    Task AddAsync(CreateUserRequest request);
    Task UpdateAsync(int id, UpdateUserRequest request);
    Task DeleteAsync(int id);
}