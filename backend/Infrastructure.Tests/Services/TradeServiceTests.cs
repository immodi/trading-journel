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
        PointValue = 1,
        EntryTime = DateTime.UtcNow,
        ExitTime = DateTime.UtcNow,
        CommissionPerUnit = 1,
        Commission = 1,
        ProfitLoss = 9
    };

    private static CreateTradeRequest CreateRequest(
        decimal commissionPerUnit = 1m,
        int quantity = 1,
        int marketId = MarketId,
        decimal pointValue = 1m) => new()
    {
        Instrument = "MNQ",
        Direction = Direction.Long,
        EntryPrice = 100,
        ExitPrice = 110,
        Quantity = quantity,
        PointValue = pointValue,
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
    public async Task AddAsync_Should_Multiply_The_Price_Move_By_The_Point_Value()
    {
        // Arrange
        Trade? added = null;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Trade>()))
            .Callback<Trade>(trade => added = trade);

        // (110 - 100) * 3 * 2 - 6
        var request = CreateRequest(commissionPerUnit: 2m, quantity: 3, pointValue: 2m);

        // Act
        await _service.AddAsync(UserId, request);

        // Assert
        added!.PointValue.Should().Be(2m);
        added.ProfitLoss.Should().Be(54m);
    }

    [Fact]
    public async Task AddAsync_Should_Throw_When_Point_Value_Is_Not_Positive()
    {
        // Arrange
        var request = CreateRequest(pointValue: 0m);

        // Act
        var act = () => _service.AddAsync(UserId, request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Trade>()), Times.Never);
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

    private static ImportTradeItem CreateImportItem(
        string instrument = "MNQ",
        Direction direction = Direction.Long,
        int quantity = 1,
        decimal entryPrice = 100,
        decimal exitPrice = 110,
        DateTime? entryTime = null,
        DateTime? exitTime = null)
    {
        var entered = entryTime ?? DateTime.UtcNow;

        return new ImportTradeItem
        {
            Instrument = instrument,
            Direction = direction,
            Quantity = quantity,
            EntryPrice = entryPrice,
            ExitPrice = exitPrice,
            EntryTime = entered,
            ExitTime = exitTime ?? entered,
        };
    }

    private static ImportTradesRequest CreateImportRequest(
        decimal commissionPerSide = 1m,
        int quantity = 1,
        int marketId = MarketId,
        int tradeCount = 1,
        decimal pointValue = 1m)
    {
        var trades = Enumerable.Range(0, tradeCount)
            .Select(_ => CreateImportItem(quantity: quantity))
            .ToList();

        return new ImportTradesRequest
        {
            MarketId = marketId,
            PointValue = pointValue,
            CommissionPerSide = commissionPerSide,
            Trades = trades,
        };
    }

    private static ImportTradesRequest CreateSingleImportRequest(
        ImportTradeItem item,
        decimal commissionPerSide = 1m,
        decimal pointValue = 1m) =>
        new()
        {
            MarketId = MarketId,
            PointValue = pointValue,
            CommissionPerSide = commissionPerSide,
            Trades = new[] { item },
        };

    [Fact]
    public async Task ImportAsync_Should_Add_Every_Trade_Under_The_Selected_Market()
    {
        // Arrange
        var request = CreateImportRequest(tradeCount: 3);

        // Act
        var imported = await _service.ImportAsync(UserId, request);

        // Assert
        imported.Should().Be(3);

        _repositoryMock.Verify(
            r => r.AddRangeAsync(It.Is<IEnumerable<Trade>>(trades =>
                trades.Count() == 3 &&
                trades.All(t => t.UserId == UserId && t.MarketId == MarketId))),
            Times.Once);

        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ImportAsync_Should_Double_The_Per_Side_Commission_For_The_Round_Trip()
    {
        // Arrange
        List<Trade>? imported = null;
        _repositoryMock
            .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<Trade>>()))
            .Callback<IEnumerable<Trade>>(trades => imported = trades.ToList());

        // Commission per side of 1 on a quantity of 2 => 1 * 2 * 2 = 4.
        var request = CreateImportRequest(commissionPerSide: 1m, quantity: 2);

        // Act
        await _service.ImportAsync(UserId, request);

        // Assert
        imported.Should().NotBeNull();
        var trade = imported!.Single();
        trade.CommissionPerUnit.Should().Be(2m);
        trade.Commission.Should().Be(4m);
        // (110 - 100) * 2 - 4
        trade.ProfitLoss.Should().Be(16m);
    }

    [Fact]
    public async Task ImportAsync_Should_Use_The_Meta_Point_Value_And_Commission()
    {
        // Arrange
        List<Trade>? imported = null;
        _repositoryMock
            .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<Trade>>()))
            .Callback<IEnumerable<Trade>>(trades => imported = trades.ToList());

        // MNQ sample: price move -20, point value 2, per-side commission 1.
        var item = CreateImportItem(entryPrice: 14824.72m, exitPrice: 14804.72m);
        var request = CreateSingleImportRequest(
            item, commissionPerSide: 1m, pointValue: 2m);

        // Act
        await _service.ImportAsync(UserId, request);

        // Assert
        imported.Should().NotBeNull();
        var trade = imported!.Single();
        trade.PointValue.Should().Be(2m);
        trade.CommissionPerUnit.Should().Be(2m);
        trade.Commission.Should().Be(2m);
        // -20 * 2 * 1 - 2
        trade.ProfitLoss.Should().Be(-42m);
    }

    [Fact]
    public async Task ImportAsync_Should_Throw_When_Point_Value_Is_Not_Positive()
    {
        // Arrange
        var request = CreateImportRequest(pointValue: 0m);

        // Act
        var act = () => _service.ImportAsync(UserId, request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();

        _repositoryMock.Verify(
            r => r.AddRangeAsync(It.IsAny<IEnumerable<Trade>>()), Times.Never);
    }

    [Fact]
    public async Task ImportAsync_Should_Return_Zero_For_An_Empty_Batch()
    {
        // Arrange
        var request = CreateImportRequest(tradeCount: 0);

        // Act
        var imported = await _service.ImportAsync(UserId, request);

        // Assert
        imported.Should().Be(0);

        _repositoryMock.Verify(
            r => r.AddRangeAsync(It.IsAny<IEnumerable<Trade>>()), Times.Never);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ImportAsync_Should_Throw_When_Market_Does_Not_Belong_To_The_User()
    {
        // Arrange
        const int foreignMarketId = 55;
        _marketRepositoryMock
            .Setup(r => r.GetByIdAsync(foreignMarketId))
            .ReturnsAsync(new Market { Id = foreignMarketId, UserId = UserId + 1 });

        var request = CreateImportRequest(marketId: foreignMarketId);

        // Act
        var act = () => _service.ImportAsync(UserId, request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();

        _repositoryMock.Verify(
            r => r.AddRangeAsync(It.IsAny<IEnumerable<Trade>>()), Times.Never);
    }

    [Fact]
    public async Task ImportAsync_Should_Throw_When_Commission_Per_Side_Is_Negative()
    {
        // Arrange
        var request = CreateImportRequest(commissionPerSide: -1m);

        // Act
        var act = () => _service.ImportAsync(UserId, request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();

        _repositoryMock.Verify(
            r => r.AddRangeAsync(It.IsAny<IEnumerable<Trade>>()), Times.Never);
    }

    [Fact]
    public async Task ImportAsync_Should_Throw_When_Quantity_Is_Not_Positive()
    {
        // Arrange
        var request = CreateSingleImportRequest(CreateImportItem(quantity: 0));

        // Act
        var act = () => _service.ImportAsync(UserId, request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();

        _repositoryMock.Verify(
            r => r.AddRangeAsync(It.IsAny<IEnumerable<Trade>>()), Times.Never);
    }

    [Fact]
    public async Task ImportAsync_Should_Throw_When_Exit_Is_Before_Entry()
    {
        // Arrange
        var entry = DateTime.UtcNow;
        var request = CreateSingleImportRequest(
            CreateImportItem(entryTime: entry, exitTime: entry.AddMinutes(-1)));

        // Act
        var act = () => _service.ImportAsync(UserId, request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();

        _repositoryMock.Verify(
            r => r.AddRangeAsync(It.IsAny<IEnumerable<Trade>>()), Times.Never);
    }

    [Fact]
    public async Task ImportAsync_Should_Throw_When_Direction_Is_Not_Tradable()
    {
        // Arrange
        var request = CreateSingleImportRequest(
            CreateImportItem(direction: Direction.Sideways));

        // Act
        var act = () => _service.ImportAsync(UserId, request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();

        _repositoryMock.Verify(
            r => r.AddRangeAsync(It.IsAny<IEnumerable<Trade>>()), Times.Never);
    }

    [Fact]
    public async Task ImportAsync_Should_Throw_When_A_Price_Is_Negative()
    {
        // Arrange
        var request = CreateSingleImportRequest(CreateImportItem(entryPrice: -1));

        // Act
        var act = () => _service.ImportAsync(UserId, request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();

        _repositoryMock.Verify(
            r => r.AddRangeAsync(It.IsAny<IEnumerable<Trade>>()), Times.Never);
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
    public async Task UpdateAsync_Should_Recompute_ProfitLoss_When_Point_Value_Changes()
    {
        // Arrange
        var trade = CreateTrade();
        trade.CommissionPerUnit = 0m;
        trade.Commission = 0m;

        _repositoryMock.Setup(r => r.GetWithMarketAsync(1)).ReturnsAsync(trade);

        // Act
        await _service.UpdateAsync(UserId, 1, new UpdateTradeRequest { PointValue = 2m });

        // Assert
        trade.PointValue.Should().Be(2m);
        // (110 - 100) * 2
        trade.ProfitLoss.Should().Be(20m);
    }

    [Fact]
    public async Task UpdateAsync_Should_Throw_When_Point_Value_Is_Not_Positive()
    {
        // Arrange
        var trade = CreateTrade();
        _repositoryMock.Setup(r => r.GetWithMarketAsync(1)).ReturnsAsync(trade);

        // Act
        var act = () => _service.UpdateAsync(UserId, 1, new UpdateTradeRequest { PointValue = 0m });

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();

        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Trade>()), Times.Never);
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
