using Domain.Entities;
using Domain.Enum;
using FluentAssertions;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Persistence;

public class UserRepositoryTests
{
    [Fact]
    public async Task AddAsync_Should_Add_User_To_Database()
    {
        await using var context = DbContextFactory.Create();
        var repository = new UserRepository(context);

        var user = new User
        {
            Username = "johndoe",
            Email = "john@example.com",
            PasswordHash = "hashed-password"
        };

        await repository.AddAsync(user);
        await repository.SaveChangesAsync();

        var savedUser = await context.Users.FirstOrDefaultAsync();

        savedUser.Should().NotBeNull();
        savedUser!.Username.Should().Be("johndoe");
        savedUser.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_User()
    {
        await using var context = DbContextFactory.Create();
        var repository = new UserRepository(context);

        var user = new User
        {
            Username = "findme",
            Email = "findme@test.com",
            PasswordHash = "hash"
        };

        await repository.AddAsync(user);
        await repository.SaveChangesAsync();

        var result = await repository.GetByIdAsync(user.Id);

        result.Should().NotBeNull();
        result!.Username.Should().Be("findme");
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_All_Users()
    {
        await using var context = DbContextFactory.Create();
        var repository = new UserRepository(context);

        await repository.AddAsync(new User
        {
            Username = "u1",
            Email = "u1@test.com",
            PasswordHash = "hash"
        });

        await repository.AddAsync(new User
        {
            Username = "u2",
            Email = "u2@test.com",
            PasswordHash = "hash"
        });

        await repository.SaveChangesAsync();

        var result = await repository.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_User()
    {
        await using var context = DbContextFactory.Create();
        var repository = new UserRepository(context);

        var user = new User
        {
            Username = "old",
            Email = "old@test.com",
            PasswordHash = "hash"
        };

        await repository.AddAsync(user);
        await repository.SaveChangesAsync();

        user.Username = "new";

        await repository.UpdateAsync(user);
        await repository.SaveChangesAsync();

        var updated = await context.Users.FindAsync(user.Id);

        updated.Should().NotBeNull();
        updated!.Username.Should().Be("new");
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_User()
    {
        await using var context = DbContextFactory.Create();
        var repository = new UserRepository(context);

        var user = new User
        {
            Username = "delete",
            Email = "delete@test.com",
            PasswordHash = "hash"
        };

        await repository.AddAsync(user);
        await repository.SaveChangesAsync();

        await repository.DeleteAsync(user.Id);
        await repository.SaveChangesAsync();

        var deleted = await context.Users.FindAsync(user.Id);

        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_Should_Cascade_To_Markets_And_Trades()
    {
        await using var context = DbContextFactory.Create();
        var repository = new UserRepository(context);

        var user = new User
        {
            Username = "cascade",
            Email = "cascade@test.com",
            PasswordHash = "hash"
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var market = new Market { UserId = user.Id, Name = "US Equities", Currency = "USD" };
        context.Markets.Add(market);
        await context.SaveChangesAsync();

        context.Trades.Add(new Trade
        {
            UserId = user.Id,
            MarketId = market.Id,
            Instrument = "MNQ",
            Direction = Direction.Long,
            EntryPrice = 100,
            ExitPrice = 110,
            Quantity = 1,
            EntryTime = DateTime.UtcNow,
            ExitTime = DateTime.UtcNow,
            CommissionPerUnit = 1,
            Commission = 1,
            ProfitLoss = 9
        });
        await context.SaveChangesAsync();

        // The API loads and deletes the user in a fresh request scope, so no
        // related entities are tracked; the database performs the cascade.
        context.ChangeTracker.Clear();

        await repository.DeleteAsync(user.Id);
        await repository.SaveChangesAsync();

        (await context.Users.FindAsync(user.Id)).Should().BeNull();
        (await context.Markets.CountAsync()).Should().Be(0);
        (await context.Trades.CountAsync()).Should().Be(0);
    }
}