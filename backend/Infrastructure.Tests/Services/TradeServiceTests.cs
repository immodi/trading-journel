using Application.Requests.Trade;
using Domain.Entities;
using Domain.Enum;
using FluentAssertions;
using Infrastructure.Services;
using Moq;
using Application.Interfaces.Repositories;

namespace Infrastructure.Tests.Services;

public class TradeServiceTests
{
    private readonly Mock<ITradeRepository> _repositoryMock = new();
    private readonly TradeService _service;

    public TradeServiceTests()
    {
        _service = new TradeService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_All_Trades()
    {
        // Arrange
        var trades = new List<Trade>
        {
            new()
            {
                Id = 1,
                Instrument = "MNQ",
                Direction = Direction.Long,
                EntryPrice = 100,
                ExitPrice = 110,
                Quantity = 1,
                EntryTime = DateTime.UtcNow,
                ExitTime = DateTime.UtcNow,
                Commission = 1,
                ProfitLoss = 9
            },
            new()
            {
                Id = 2,
                Instrument = "NQ",
                Direction = Direction.Short,
                EntryPrice = 200,
                ExitPrice = 190,
                Quantity = 1,
                EntryTime = DateTime.UtcNow,
                ExitTime = DateTime.UtcNow,
                Commission = 1,
                ProfitLoss = 9
            }
        };

        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(trades);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Trade()
    {
        // Arrange
        var trade = new Trade
        {
            Id = 1,
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

        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(trade);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Instrument.Should().Be("MNQ");
    }

    [Fact]
    public async Task AddAsync_Should_Add_Trade()
    {
        // Arrange
        var request = new CreateTradeRequest
        {
            UserId = 1,
            Instrument = "MNQ",
            Direction = Direction.Long,
            EntryPrice = 100,
            ExitPrice = 110,
            Quantity = 1,
            EntryTime = DateTime.UtcNow,
            ExitTime = DateTime.UtcNow,
        };

        // Act
        await _service.AddAsync(request);

        // Assert
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Trade>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Trade()
    {
        // Arrange
        var trade = new Trade
        {
            Id = 1,
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
        await _service.UpdateAsync(1, request);

        // Assert
        trade.Instrument.Should().Be("NEW");

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
        var act = () => _service.UpdateAsync(1, request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_Should_Delete_Trade()
    {
        // Act
        await _service.DeleteAsync(1);

        // Assert
        _repositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}