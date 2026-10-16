using Domain.Entities;
using Domain.Enum;
using FluentAssertions;
using Infrastructure.Persistence.Repositories;

namespace Infrastructure.Tests.Persistence;

public class MarketRepositoryTests
{
    private static User CreateUser(string username = "test") => new()
    {
        Username = username,
        Email = $"{username}@test.com",
        PasswordHash = "hash"
    };

    [Fact]
    public async Task AddAsync_Should_Add_Market()
    {
        await using var context = DbContextFactory.Create();
        var repository = new MarketRepository(context);

        var user = CreateUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var market = new Market { UserId = user.Id, Name = "US Equities", Currency = "USD" };

        await repository.AddAsync(market);
        await repository.SaveChangesAsync();

        var saved = await context.Markets.FindAsync(market.Id);

        saved.Should().NotBeNull();
        saved!.Name.Should().Be("US Equities");
        saved.Currency.Should().Be("USD");
    }

    [Fact]
    public async Task GetOwnedAsync_Should_Return_Market_With_Trades()
    {
        await using var context = DbContextFactory.Create();
        var repository = new MarketRepository(context);

        var user = CreateUser();
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

        var result = await repository.GetOwnedAsync(user.Id, market.Id);

        result.Should().NotBeNull();
        result!.Trades.Should().ContainSingle();
    }

    [Fact]
    public async Task GetOwnedAsync_Should_Return_Null_When_Owned_By_Another_User()
    {
        await using var context = DbContextFactory.Create();
        var repository = new MarketRepository(context);

        var owner = CreateUser("owner");
        var other = CreateUser("other");
        context.Users.AddRange(owner, other);
        await context.SaveChangesAsync();

        var market = new Market { UserId = owner.Id, Name = "US Equities", Currency = "USD" };
        context.Markets.Add(market);
        await context.SaveChangesAsync();

        var result = await repository.GetOwnedAsync(other.Id, market.Id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_Only_Return_That_Users_Markets()
    {
        await using var context = DbContextFactory.Create();
        var repository = new MarketRepository(context);

        var owner = CreateUser("owner");
        var other = CreateUser("other");
        context.Users.AddRange(owner, other);
        await context.SaveChangesAsync();

        context.Markets.AddRange(
            new Market { UserId = owner.Id, Name = "Owned", Currency = "USD" },
            new Market { UserId = other.Id, Name = "Foreign", Currency = "USD" });
        await context.SaveChangesAsync();

        var result = (await repository.GetByUserIdAsync(owner.Id)).ToList();

        result.Should().ContainSingle();
        result.Single().Name.Should().Be("Owned");
    }

    [Fact]
    public async Task NameExistsAsync_Should_Detect_Duplicates_And_Honor_Exclusion()
    {
        await using var context = DbContextFactory.Create();
        var repository = new MarketRepository(context);

        var user = CreateUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var market = new Market { UserId = user.Id, Name = "US Equities", Currency = "USD" };
        context.Markets.Add(market);
        await context.SaveChangesAsync();

        (await repository.NameExistsAsync(user.Id, "US Equities")).Should().BeTrue();
        (await repository.NameExistsAsync(user.Id, "Crypto")).Should().BeFalse();
        (await repository.NameExistsAsync(user.Id, "US Equities", market.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_Market_Without_Trades()
    {
        await using var context = DbContextFactory.Create();
        var repository = new MarketRepository(context);

        var user = CreateUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var market = new Market { UserId = user.Id, Name = "US Equities", Currency = "USD" };
        context.Markets.Add(market);
        await context.SaveChangesAsync();

        await repository.DeleteAsync(market.Id);
        await repository.SaveChangesAsync();

        (await context.Markets.FindAsync(market.Id)).Should().BeNull();
    }
}
