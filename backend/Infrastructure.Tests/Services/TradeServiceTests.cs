using Application.Interfaces.Repositories;
using Application.Requests.Trade;
using Domain.Entities;
using Domain.Enum;
using FluentAssertions;
using Infrastructure.Services;
using Moq;

namespace Infrastructure.Tests.Services;

public class TradeServiceTests
{
    private const int UserId = 1;
    private const int MarketId = 10;

    private readonly Mock<ITradeRepository> _repositoryMock = new();
    private readonly Mock<IMarketRepository> _marketRepositoryMock = new();
    private readonly TradeService _service;

    public TradeServiceTests()
    {
        _service = new TradeService(_repositoryMock.Object, _marketRepositoryMock.Object);

        // By default every test can resolve the caller's market.
        _marketRepositoryMock
            .Setup(r => r.GetByIdAsync(MarketId))
            .ReturnsAsync(CreateMarket());
    }

    private static Market CreateMarket(int id = MarketId, int userId = UserId) => new()
    {
        Id = id,
        UserId = userId,
        Name = "US Equities",
        Currency = "USD",
    };

    private static Trade CreateTrade(int id = 1, int userId = UserId, string instrument = "MNQ") => new()
    {
        Id = id,
        UserId = userId,
        MarketId = MarketId,
        Market = CreateMarket(),
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

    private static CreateTradeRequest CreateRequest(
        decimal commissionPerUnit = 1m,
        int quantity = 1,
        int marketId = MarketId) => new()
    {
        Instrument = "MNQ",
        Direction = Direction.Long,
        EntryPrice = 100,
        ExitPrice = 110,
        Quantity = quantity,
        EntryTime = DateTime.UtcNow,
        ExitTime = DateTime.UtcNow,
        MarketId = marketId,
        CommissionPerUnit = commissionPerUnit,
    };

    [Fact]
    public async Task GetAllAsync_Should_Return_Trades_For_The_User()
    {
        // Arrange
        var trades = new List<Trade>
        {
            CreateTrade(1, UserId, "MNQ"),
            CreateTrade(2, UserId, "NQ")
        };

        _repositoryMock
            .Setup(r => r.GetByUserIdAsync(UserId))
            .ReturnsAsync(trades);

        // Act
        var result = await _service.GetAllAsync(UserId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(t => t.Currency == "USD");

        _repositoryMock.Verify(r => r.GetByUserIdAsync(UserId), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_Should_Preserve_Each_Trades_Market_Currency()
    {
        // Arrange
        var usdMarket = new Market { Id = 10, UserId = UserId, Name = "US Equities", Currency = "USD" };
        var egpMarket = new Market { Id = 11, UserId = UserId, Name = "Egyptian Stocks", Currency = "EGP" };

        var usdTrade = CreateTrade(1, UserId, "AAPL");
        usdTrade.MarketId = usdMarket.Id;
        usdTrade.Market = usdMarket;

        var egpTrade = CreateTrade(2, UserId, "COMI");
        egpTrade.MarketId = egpMarket.Id;
        egpTrade.Market = egpMarket;

        _repositoryMock
            .Setup(r => r.GetByUserIdAsync(UserId))
            .ReturnsAsync(new List<Trade> { usdTrade, egpTrade });

        // Act
        var result = (await _service.GetAllAsync(UserId)).ToList();

        // Assert — each trade reports the currency of its own market and the
        // two are never collapsed into a single value.
        result.Should().HaveCount(2);
        result.Single(t => t.Instrument == "AAPL").Currency.Should().Be("USD");
        result.Single(t => t.Instrument == "COMI").Currency.Should().Be("EGP");
        result.Select(t => t.Currency).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Trade_With_Inherited_Currency()
    {
        // Arrange
        var trade = CreateTrade();

        _repositoryMock.Setup(r => r.GetWithMarketAsync(1)).ReturnsAsync(trade);

        // Act
        var result = await _service.GetByIdAsync(UserId, 1);

        // Assert
        result.Should().NotBeNull();
        result!.Instrument.Should().Be("MNQ");
        result.MarketId.Should().Be(MarketId);
        result.MarketName.Should().Be("US Equities");
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public async Task GetByIdAsync_Should_Throw_When_Trade_Belongs_To_Another_User()
    {
        // Arrange
        var trade = CreateTrade(userId: UserId + 1);

        _repositoryMock.Setup(r => r.GetWithMarketAsync(1)).ReturnsAsync(trade);

        // Act
        var act = () => _service.GetByIdAsync(UserId, 1);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task AddAsync_Should_Add_Trade_Owned_By_The_User()
    {
        // Arrange
        var request = CreateRequest();

        // Act
        await _service.AddAsync(UserId, request);

        // Assert
        _repositoryMock.Verify(
            r => r.AddAsync(It.Is<Trade>(t =>
                t.UserId == UserId &&
                t.MarketId == MarketId &&
                t.Instrument == "MNQ")),
            Times.Once);

        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddAsync_Should_Compute_Total_Commission_From_Per_Unit_Rate()
    {
        // Arrange
        Trade? added = null;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Trade>()))
            .Callback<Trade>(trade => added = trade);

        var request = CreateRequest(commissionPerUnit: 2.5m, quantity: 3);

        // Act
        await _service.AddAsync(UserId, request);

        // Assert
        added.Should().NotBeNull();
        added!.CommissionPerUnit.Should().Be(2.5m);
        added.Commission.Should().Be(7.5m);

        // (110 - 100) * 3 - 7.5
        added.ProfitLoss.Should().Be(22.5m);
    }

    [Fact]
    public async Task AddAsync_Should_Allow_Zero_Commission()
    {
        // Arrange
        Trade? added = null;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Trade>()))
            .Callback<Trade>(trade => added = trade);

        var request = CreateRequest(commissionPerUnit: 0m, quantity: 4);

        // Act
        await _service.AddAsync(UserId, request);

        // Assert
        added!.Commission.Should().Be(0m);
        added.ProfitLoss.Should().Be(40m);
    }

    [Fact]
    public async Task AddAsync_Should_Throw_When_Market_Does_Not_Exist()
    {
        // Arrange
        const int missingMarketId = 999;
        _marketRepositoryMock
            .Setup(r => r.GetByIdAsync(missingMarketId))
            .ReturnsAsync((Market?)null);

        var request = CreateRequest(marketId: missingMarketId);

        // Act
        var act = () => _service.AddAsync(UserId, request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Trade>()), Times.Never);
    }

    [Fact]
    public async Task AddAsync_Should_Throw_When_Market_Belongs_To_Another_User()
    {
        // Arrange
        const int foreignMarketId = 55;
        _marketRepositoryMock
            .Setup(r => r.GetByIdAsync(foreignMarketId))
            .ReturnsAsync(new Market { Id = foreignMarketId, UserId = UserId + 1 });

        var request = CreateRequest(marketId: foreignMarketId);

        // Act
        var act = () => _service.AddAsync(UserId, request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task AddAsync_Should_Throw_When_Commission_Per_Unit_Is_Negative()
    {
        // Arrange
        var request = CreateRequest(commissionPerUnit: -1m);

        // Act
        var act = () => _service.AddAsync(UserId, request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Trade()
    {
        // Arrange
        var trade = CreateTrade(instrument: "OLD");

        var request = new UpdateTradeRequest
        {
            Instrument = "NEW",
            Direction = Direction.Short,
            EntryPrice = 120,
            ExitPrice = 100,
            Quantity = 2,
            EntryTime = trade.EntryTime,
            ExitTime = trade.ExitTime,
            CommissionPerUnit = 2,
        };

        _repositoryMock.Setup(r => r.GetWithMarketAsync(1)).ReturnsAsync(trade);

        // Act
        await _service.UpdateAsync(UserId, 1, request);

        // Assert
        trade.Instrument.Should().Be("NEW");
        trade.CommissionPerUnit.Should().Be(2);
        trade.Commission.Should().Be(4);
        // (120 - 100) * 2 - 4
        trade.ProfitLoss.Should().Be(36);

        _repositoryMock.Verify(r => r.UpdateAsync(trade), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Should_Recompute_Total_Commission_When_Quantity_Changes()
    {
        // Arrange
        var trade = CreateTrade();
        trade.CommissionPerUnit = 3m;
        trade.Commission = 3m;

        _repositoryMock.Setup(r => r.GetWithMarketAsync(1)).ReturnsAsync(trade);

        // Act
        await _service.UpdateAsync(UserId, 1, new UpdateTradeRequest { Quantity = 4 });

        // Assert
        trade.Quantity.Should().Be(4);
        trade.Commission.Should().Be(12m);
    }

    [Fact]
    public async Task UpdateAsync_Should_Change_Market_When_Owned_By_The_User()
    {
        // Arrange
        var trade = CreateTrade();
        const int newMarketId = 20;
        var newMarket = new Market { Id = newMarketId, UserId = UserId, Name = "Crypto", Currency = "EUR" };

        _repositoryMock.Setup(r => r.GetWithMarketAsync(1)).ReturnsAsync(trade);
        _marketRepositoryMock.Setup(r => r.GetByIdAsync(newMarketId)).ReturnsAsync(newMarket);

        // Act
        await _service.UpdateAsync(UserId, 1, new UpdateTradeRequest { MarketId = newMarketId });

        // Assert
        trade.MarketId.Should().Be(newMarketId);
        trade.Market.Should().Be(newMarket);
    }

    [Fact]
    public async Task UpdateAsync_Should_Throw_When_Trade_Not_Found()
    {
        // Arrange
        var request = new UpdateTradeRequest();

        _repositoryMock.Setup(r => r.GetWithMarketAsync(1))
            .ReturnsAsync((Trade?)null);

        // Act
        var act = () => _service.UpdateAsync(UserId, 1, request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_Should_Delete_Trade()
    {
        // Arrange
        var trade = CreateTrade();

        _repositoryMock.Setup(r => r.GetWithMarketAsync(1)).ReturnsAsync(trade);

        // Act
        await _service.DeleteAsync(UserId, 1);

        // Assert
        _repositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_Should_Throw_When_Trade_Belongs_To_Another_User()
    {
        // Arrange
        var trade = CreateTrade(userId: UserId + 1);

        _repositoryMock.Setup(r => r.GetWithMarketAsync(1)).ReturnsAsync(trade);

        // Act
        var act = () => _service.DeleteAsync(UserId, 1);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();

        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }
}
