using Domain.Entities;
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
}