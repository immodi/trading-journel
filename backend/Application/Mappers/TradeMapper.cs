using Application.DTOs;
using Application.Requests.Trade;
using Domain.Entities;
using Domain.Enum;

namespace Application.Mappers;

public static class TradeMapper
{
        public static TradeDto ToDto(this Trade trade)
        {
            return new TradeDto
            {
                Id = trade.Id,
                Instrument = trade.Instrument,
                Direction = trade.Direction,
                EntryPrice = trade.EntryPrice,
                ExitPrice = trade.ExitPrice,
                Quantity = trade.Quantity,
                ProfitLoss = trade.ProfitLoss
            };
        }

        public static Trade ToEntity(this CreateTradeRequest request)
        {
            return new Trade
            {
                Instrument = request.Instrument,
                UserId = request.UserId,
                Direction = request.Direction,
                EntryPrice = request.EntryPrice,
                ExitPrice = request.ExitPrice,
                Quantity = request.Quantity,
                EntryTime = request.EntryTime,
                ExitTime = request.ExitTime,
                Commission = request.Commission,
                ProfitLoss = CalculateProfitLoss(
                    request.Direction, 
                    request.EntryPrice, 
                    request.ExitPrice, 
                    request.Quantity, 
                    request.Commission)
            };
        }
        
        public static void UpdateTrade(this UpdateTradeRequest request, Trade trade)
        {
            if (request.Instrument is not null)
                trade.Instrument = request.Instrument;

            if (request.Direction.HasValue)
                trade.Direction = request.Direction.Value;

            if (request.EntryPrice.HasValue)
                trade.EntryPrice = request.EntryPrice.Value;

            if (request.ExitPrice.HasValue)
                trade.ExitPrice = request.ExitPrice.Value;

            if (request.Quantity.HasValue)
                trade.Quantity = request.Quantity.Value;

            if (request.EntryTime.HasValue)
                trade.EntryTime = request.EntryTime.Value;

            if (request.ExitTime.HasValue)
                trade.ExitTime = request.ExitTime.Value;

            if (request.Commission.HasValue)
                trade.Commission = request.Commission.Value;

            trade.ProfitLoss = CalculateProfitLoss(
                trade.Direction,
                trade.EntryPrice,
                trade.ExitPrice,
                trade.Quantity,
                trade.Commission);

            trade.UpdatedAt = DateTime.UtcNow;
        }
        
        private static decimal CalculateProfitLoss(
            Direction direction,
            decimal entryPrice,
            decimal exitPrice,
            int quantity,
            decimal commission)
        {
            var pnl = direction switch
            {
                Direction.Long => (exitPrice - entryPrice) * quantity,
                Direction.Short => (entryPrice - exitPrice) * quantity,
                _ => throw new ArgumentOutOfRangeException(nameof(direction))
            };

            return pnl - commission;
        }
}