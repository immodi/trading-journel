using Domain.Entities;
using Domain.Enum;
using FluentAssertions;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Persistence;

public class TradeRepositoryTests
{
    private static async Task<(User User, Market Market)> SeedOwnerAsync(
        AppDbContext context,
        string username = "test")
    {
        var user = new User
        {
            Username = username,
            Email = $"{username}@test.com",
            PasswordHash = "hash"
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var market = new Market
        {
            UserId = user.Id,
            Name = "US Equities",
            Currency = "USD"
        };

        context.Markets.Add(market);
        await context.SaveChangesAsync();

        return (user, market);
    }

    private static Trade CreateTrade(int userId, int marketId, string instrument = "MNQ") => new()
    {
        UserId = userId,
        MarketId = marketId,
        Instrument = instrument,
        Direction = Direction.Long,
        EntryPrice = 100,
        ExitPrice = 110,
        Quantity = 1,
        EntryTime = DateTime.UtcNow,
        ExitTime = DateTime.UtcNow,
        CommissionPerUnit = 1,
        Commission = 1,
        ProfitLoss = 9
    };

    [Fact]
    public async Task AddAsync_Should_Add_Trade()
    {
        await using var context = DbContextFactory.Create();
        var repository = new TradeRepository(context);
        var (user, market) = await SeedOwnerAsync(context);

        await repository.AddAsync(CreateTrade(user.Id, market.Id));
        await repository.SaveChangesAsync();

        var saved = await context.Trades.FirstOrDefaultAsync();

        saved.Should().NotBeNull();
        saved!.Instrument.Should().Be("MNQ");
        saved.MarketId.Should().Be(market.Id);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Trade()
    {
        await using var context = DbContextFactory.Create();
        var repository = new TradeRepository(context);
        var (user, market) = await SeedOwnerAsync(context);

        var trade = CreateTrade(user.Id, market.Id);
        await repository.AddAsync(trade);
        await repository.SaveChangesAsync();

        var result = await repository.GetByIdAsync(trade.Id);

        result.Should().NotBeNull();
        result!.Instrument.Should().Be("MNQ");
    }

    [Fact]
    public async Task GetWithMarketAsync_Should_Include_The_Market()
    {
        await using var context = DbContextFactory.Create();
        var repository = new TradeRepository(context);
        var (user, market) = await SeedOwnerAsync(context);

        var trade = CreateTrade(user.Id, market.Id);
        await repository.AddAsync(trade);
        await repository.SaveChangesAsync();

        var result = await repository.GetWithMarketAsync(trade.Id);

        result.Should().NotBeNull();
        result!.Market.Should().NotBeNull();
        result.Market!.Currency.Should().Be("USD");
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_All_Trades()
    {
        await using var context = DbContextFactory.Create();
        var repository = new TradeRepository(context);
        var (user, market) = await SeedOwnerAsync(context);

        await repository.AddAsync(CreateTrade(user.Id, market.Id, "T1"));
        await repository.AddAsync(CreateTrade(user.Id, market.Id, "T2"));
        await repository.SaveChangesAsync();

        var result = await repository.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Trade()
    {
        await using var context = DbContextFactory.Create();
        var repository = new TradeRepository(context);
        var (user, market) = await SeedOwnerAsync(context);

        var trade = CreateTrade(user.Id, market.Id, "OLD");
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
        var (user, market) = await SeedOwnerAsync(context);

        var trade = CreateTrade(user.Id, market.Id, "DEL");
        await repository.AddAsync(trade);
        await repository.SaveChangesAsync();

        await repository.DeleteAsync(trade.Id);
        await repository.SaveChangesAsync();

        var deleted = await context.Trades.FindAsync(trade.Id);

        deleted.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_Only_Return_Trades_For_That_User()
    {
        await using var context = DbContextFactory.Create();
        var repository = new TradeRepository(context);
        var (owner, ownerMarket) = await SeedOwnerAsync(context, "owner");
        var (other, otherMarket) = await SeedOwnerAsync(context, "other");

        await repository.AddAsync(CreateTrade(owner.Id, ownerMarket.Id, "OWNED"));
        await repository.AddAsync(CreateTrade(other.Id, otherMarket.Id, "FOREIGN"));
        await repository.SaveChangesAsync();

        var result = (await repository.GetByUserIdAsync(owner.Id)).ToList();

        result.Should().ContainSingle();
        result.Single().Instrument.Should().Be("OWNED");
        result.Single().Market.Should().NotBeNull();
    }

    [Fact]
    public async Task AnyForMarketAsync_Should_Report_Associated_Trades()
    {
        await using var context = DbContextFactory.Create();
        var repository = new TradeRepository(context);
        var (user, market) = await SeedOwnerAsync(context);

        (await repository.AnyForMarketAsync(market.Id)).Should().BeFalse();

        await repository.AddAsync(CreateTrade(user.Id, market.Id));
        await repository.SaveChangesAsync();

        (await repository.AnyForMarketAsync(market.Id)).Should().BeTrue();
    }
}
