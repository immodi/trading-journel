using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class UserRepository(AppDbContext context)
    : Repository<User>(context), IUserRepository
{
    private readonly DbSet<User> _dbSet = context.Set<User>();

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.SingleOrDefaultAsync(u => u.Email == email);
    }
}