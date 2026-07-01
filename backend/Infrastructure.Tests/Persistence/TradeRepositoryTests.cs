using Domain.Entities;
using Domain.Enum;
using FluentAssertions;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Persistence;

public class TradeRepositoryTests
{
    [Fact]
    public async Task AddAsync_Should_Add_Trade()
    {
        await using var context = DbContextFactory.Create();
        var repository = new TradeRepository(context);

        var user = new User
        {
            Username = "test",
            Email = "test@test.com",
            PasswordHash = "hash"
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var trade = new Trade
        {
            UserId = user.Id,
            Instrument = "MNQ",
            Direction = Direction.Long,
            EntryPrice = 100,
            ExitPrice = 110,
            Quantity = 1,
            EntryTime = DateTime.UtcNow,
            ExitTime = DateTime.UtcNow,
            Commission = 1,
            ProfitLoss = 9
        };

        await repository.AddAsync(trade);
        await repository.SaveChangesAsync();

        var saved = await context.Trades.FirstOrDefaultAsync();

        saved.Should().NotBeNull();
        saved!.Instrument.Should().Be("MNQ");
    }
    
    
    [Fact]
    public async Task GetByIdAsync_Should_Return_Trade()
    {
        await using var context = DbContextFactory.Create();
        var repository = new TradeRepository(context);

        var user = new User
        {
            Username = "test",
            Email = "test@test.com",
            PasswordHash = "hash"
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var trade = new Trade
        {
            UserId = user.Id,
            Instrument = "MNQ",
            Direction = Direction.Long,
            EntryPrice = 100,
            ExitPrice = 110,
            Quantity = 1,
            EntryTime = DateTime.UtcNow,
            ExitTime = DateTime.UtcNow,
            Commission = 1,
            ProfitLoss = 9
        };

        await repository.AddAsync(trade);
        await repository.SaveChangesAsync();

        var result = await repository.GetByIdAsync(trade.Id);

        result.Should().NotBeNull();
        result!.Instrument.Should().Be("MNQ");
    } 
    
    
    [Fact]
    public async Task GetAllAsync_Should_Return_All_Trades()
    {
        await using var context = DbContextFactory.Create();
        var repository = new TradeRepository(context);

        var user = new User
        {
            Username = "test",
            Email = "test@test.com",
            PasswordHash = "hash"
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        await repository.AddAsync(new Trade
        {
            UserId = user.Id,
            Instrument = "T1",
            Direction = Direction.Long,
            EntryPrice = 100,
            ExitPrice = 110,
            Quantity = 1,
            EntryTime = DateTime.UtcNow,
            ExitTime = DateTime.UtcNow,
            Commission = 1,
            ProfitLoss = 9
        });

        await repository.AddAsync(new Trade
        {
            UserId = user.Id,
            Instrument = "T2",
            Direction = Direction.Short,
            EntryPrice = 200,
            ExitPrice = 190,
            Quantity = 1,
            EntryTime = DateTime.UtcNow,
            ExitTime = DateTime.UtcNow,
            Commission = 1,
            ProfitLoss = 9
        });

        await repository.SaveChangesAsync();

        var result = await repository.GetAllAsync();

        result.Should().HaveCount(2);
    }
    
    [Fact]
    public async Task UpdateAsync_Should_Update_Trade()
    {
        await using var context = DbContextFactory.Create();
        var repository = new TradeRepository(context);

        var user = new User
        {
            Username = "test",
            Email = "test@test.com",
            PasswordHash = "hash"
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var trade = new Trade
        {
            UserId = user.Id,
            Instrument = "OLD",
            Direction = Direction.Long,
            EntryPrice = 100,
            ExitPrice = 110,
            Quantity = 1,
            EntryTime = DateTime.UtcNow,
            ExitTime = DateTime.UtcNow,
            Commission = 1,
            ProfitLoss = 9
        };

        await repository.AddAsync(trade);
        await repository.SaveChangesAsync();

        trade.Instrument = "NEW";

        await repository.UpdateAsync(trade);
        await repository.SaveChangesAsync();

        var updated = await context.Trades.FindAsync(trade.Id);

        updated!.Instrument.Should().Be("NEW");
    }
    
    [Fact]
    public async Task DeleteAsync_Should_Remove_Trade()
    {
        await using var context = DbContextFactory.Create();
        var repository = new TradeRepository(context);

        var user = new User
        {
            Username = "test",
            Email = "test@test.com",
            PasswordHash = "hash"
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var trade = new Trade
        {
            UserId = user.Id,
            Instrument = "DEL",
            Direction = Direction.Long,
            EntryPrice = 100,
            ExitPrice = 110,
            Quantity = 1,
            EntryTime = DateTime.UtcNow,
            ExitTime = DateTime.UtcNow,
            Commission = 1,
            ProfitLoss = 9
        };

        await repository.AddAsync(trade);
        await repository.SaveChangesAsync();

        await repository.DeleteAsync(trade.Id);
        await repository.SaveChangesAsync();

        var deleted = await context.Trades.FindAsync(trade.Id);

        deleted.Should().BeNull();
    }
}