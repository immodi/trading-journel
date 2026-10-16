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

    private readonly Mock<ITradeRepository> _repositoryMock = new();
    private readonly TradeService _service;

    public TradeServiceTests()
    {
        _service = new TradeService(_repositoryMock.Object);
    }

    private static Trade CreateTrade(int id = 1, int userId = UserId, string instrument = "MNQ") => new()
    {
        Id = id,
        UserId = userId,
        Instrument = instrument,
        Direction = Direction.Long,
        EntryPrice = 100,
        ExitPrice = 110,
        Quantity = 1,
        EntryTime = DateTime.UtcNow,
        ExitTime = DateTime.UtcNow,
        Commission = 1,
        ProfitLoss = 9
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

        _repositoryMock.Verify(r => r.GetByUserIdAsync(UserId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Trade()
    {
        // Arrange
        var trade = CreateTrade();

        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(trade);

        // Act
        var result = await _service.GetByIdAsync(UserId, 1);

        // Assert
        result.Should().NotBeNull();
        result!.Instrument.Should().Be("MNQ");
    }

    [Fact]
    public async Task GetByIdAsync_Should_Throw_When_Trade_Belongs_To_Another_User()
    {
        // Arrange
        var trade = CreateTrade(userId: UserId + 1);

        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(trade);

        // Act
        var act = () => _service.GetByIdAsync(UserId, 1);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task AddAsync_Should_Add_Trade_Owned_By_The_User()
    {
        // Arrange
        var request = new CreateTradeRequest
        {
            Instrument = "MNQ",
            Direction = Direction.Long,
            EntryPrice = 100,
            ExitPrice = 110,
            Quantity = 1,
            EntryTime = DateTime.UtcNow,
            ExitTime = DateTime.UtcNow,
        };

        // Act
        await _service.AddAsync(UserId, request);

        // Assert
        _repositoryMock.Verify(
            r => r.AddAsync(It.Is<Trade>(t => t.UserId == UserId && t.Instrument == "MNQ")),
            Times.Once);

        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddAsync_Should_Use_The_Provided_Commission()
    {
        // Arrange
        Trade? added = null;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Trade>()))
            .Callback<Trade>(trade => added = trade);

        var request = new CreateTradeRequest
        {
            Instrument = "MNQ",
            Direction = Direction.Long,
            EntryPrice = 100,
            ExitPrice = 110,
            Quantity = 2,
            EntryTime = DateTime.UtcNow,
            ExitTime = DateTime.UtcNow,
            Commission = 5m,
        };

        // Act
        await _service.AddAsync(UserId, request);

        // Assert
        added.Should().NotBeNull();
        added!.Commission.Should().Be(5m);

        // (110 - 100) * 2 - 5
        added.ProfitLoss.Should().Be(15m);
    }

    [Fact]
    public async Task AddAsync_Should_Default_Commission_When_Not_Provided()
    {
        // Arrange
        Trade? added = null;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Trade>()))
            .Callback<Trade>(trade => added = trade);

        var request = new CreateTradeRequest
        {
            Instrument = "MNQ",
            Direction = Direction.Long,
            EntryPrice = 100,
            ExitPrice = 110,
            Quantity = 3,
            EntryTime = DateTime.UtcNow,
            ExitTime = DateTime.UtcNow,
        };

        // Act
        await _service.AddAsync(UserId, request);

        // Assert
        added.Should().NotBeNull();
        added!.Commission.Should().Be(1.82m * 3);
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
            Commission = 2,
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(trade);

        // Act
        await _service.UpdateAsync(UserId, 1, request);

        // Assert
        trade.Instrument.Should().Be("NEW");
        trade.Commission.Should().Be(2);
        // (120 - 100) * 2 - 2
        trade.ProfitLoss.Should().Be(38);

        _repositoryMock.Verify(r => r.UpdateAsync(trade), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Should_Throw_When_Trade_Not_Found()
    {
        // Arrange
        var request = new UpdateTradeRequest();

        _repositoryMock.Setup(r => r.GetByIdAsync(1))
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

        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(trade);

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

        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(trade);

        // Act
        var act = () => _service.DeleteAsync(UserId, 1);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();

        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }
}
