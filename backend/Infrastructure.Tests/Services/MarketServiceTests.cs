using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Requests.Market;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Services;
using Moq;

namespace Infrastructure.Tests.Services;

public class MarketServiceTests
{
    private const int UserId = 1;

    private readonly Mock<IMarketRepository> _repositoryMock = new();
    private readonly MarketService _service;

    public MarketServiceTests()
    {
        _service = new MarketService(_repositoryMock.Object);
    }

    private static Market CreateMarket(
        int id = 1,
        int userId = UserId,
        string name = "US Equities",
        string currency = "USD",
        params Trade[] trades) => new()
    {
        Id = id,
        UserId = userId,
        Name = name,
        Currency = currency,
        Trades = trades,
    };

    [Fact]
    public async Task GetAllAsync_Should_Return_Markets_For_The_User()
    {
        // Arrange
        var markets = new List<Market>
        {
            CreateMarket(1, UserId, "US Equities", "USD", new Trade { Id = 1 }),
            CreateMarket(2, UserId, "Crypto", "EUR"),
        };

        _repositoryMock.Setup(r => r.GetByUserIdAsync(UserId)).ReturnsAsync(markets);

        // Act
        var result = (await _service.GetAllAsync(UserId)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Single(m => m.Id == 1).TradeCount.Should().Be(1);
        result.Single(m => m.Id == 2).TradeCount.Should().Be(0);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Market()
    {
        // Arrange
        var market = CreateMarket();

        _repositoryMock.Setup(r => r.GetOwnedAsync(UserId, 1)).ReturnsAsync(market);

        // Act
        var result = await _service.GetByIdAsync(UserId, 1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("US Equities");
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public async Task AddAsync_Should_Create_Market_And_Normalize_Currency()
    {
        // Arrange
        Market? added = null;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Market>()))
            .Callback<Market>(market => added = market);

        var request = new CreateMarketRequest { Name = "  US Equities ", Currency = "usd" };

        // Act
        var result = await _service.AddAsync(UserId, request);

        // Assert
        added.Should().NotBeNull();
        added!.UserId.Should().Be(UserId);
        added.Name.Should().Be("US Equities");
        added.Currency.Should().Be("USD");
        result.Name.Should().Be("US Equities");

        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddAsync_Should_Throw_When_Name_Already_Exists()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.NameExistsAsync(UserId, "US Equities", null))
            .ReturnsAsync(true);

        var request = new CreateMarketRequest { Name = "US Equities", Currency = "USD" };

        // Act
        var act = () => _service.AddAsync(UserId, request);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Market>()), Times.Never);
    }

    [Theory]
    [InlineData("", "USD")]
    [InlineData("   ", "USD")]
    [InlineData("US Equities", "")]
    [InlineData("US Equities", "US")]
    [InlineData("US Equities", "US1")]
    public async Task AddAsync_Should_Throw_On_Invalid_Input(string name, string currency)
    {
        // Arrange
        var request = new CreateMarketRequest { Name = name, Currency = currency };

        // Act
        var act = () => _service.AddAsync(UserId, request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Market()
    {
        // Arrange
        var market = CreateMarket();

        _repositoryMock.Setup(r => r.GetOwnedAsync(UserId, 1)).ReturnsAsync(market);

        var request = new UpdateMarketRequest { Name = "Futures", Currency = "gbp" };

        // Act
        await _service.UpdateAsync(UserId, 1, request);

        // Assert
        market.Name.Should().Be("Futures");
        market.Currency.Should().Be("GBP");

        _repositoryMock.Verify(r => r.UpdateAsync(market), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Should_Throw_When_Market_Not_Found()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetOwnedAsync(UserId, 1)).ReturnsAsync((Market?)null);

        // Act
        var act = () => _service.UpdateAsync(UserId, 1, new UpdateMarketRequest { Name = "X" });

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_Should_Throw_When_Renaming_To_Existing_Name()
    {
        // Arrange
        var market = CreateMarket();
        _repositoryMock.Setup(r => r.GetOwnedAsync(UserId, 1)).ReturnsAsync(market);
        _repositoryMock
            .Setup(r => r.NameExistsAsync(UserId, "Crypto", 1))
            .ReturnsAsync(true);

        // Act
        var act = () => _service.UpdateAsync(UserId, 1, new UpdateMarketRequest { Name = "Crypto" });

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task DeleteAsync_Should_Delete_Market_Without_Trades()
    {
        // Arrange
        var market = CreateMarket();
        _repositoryMock.Setup(r => r.GetOwnedAsync(UserId, 1)).ReturnsAsync(market);

        // Act
        await _service.DeleteAsync(UserId, 1);

        // Assert
        _repositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_Should_Refuse_When_Market_Has_Trades()
    {
        // Arrange
        var market = CreateMarket(trades: [new Trade { Id = 1 }, new Trade { Id = 2 }]);
        _repositoryMock.Setup(r => r.GetOwnedAsync(UserId, 1)).ReturnsAsync(market);

        // Act
        var act = () => _service.DeleteAsync(UserId, 1);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();

        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_Should_Throw_When_Market_Not_Found()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetOwnedAsync(UserId, 1)).ReturnsAsync((Market?)null);

        // Act
        var act = () => _service.DeleteAsync(UserId, 1);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
