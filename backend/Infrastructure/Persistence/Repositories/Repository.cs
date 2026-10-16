using Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class Repository<T>(AppDbContext context) : IRepository<T>
    where T : class
{
    protected AppDbContext Context { get; } = context;

    private DbSet<T> Set => Context.Set<T>();

    public async Task<IEnumerable<T>> GetAllAsync() => await Set.ToListAsync();

    public async Task<T?> GetByIdAsync(int id) => await Set.FindAsync(id);

    public async Task AddAsync(T entity) => await Set.AddAsync(entity);

    public async Task UpdateAsync(T entity) => await Task.FromResult(Set.Update(entity));

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null) Set.Remove(entity);
    }

    public async Task SaveChangesAsync() => await Context.SaveChangesAsync();
}
