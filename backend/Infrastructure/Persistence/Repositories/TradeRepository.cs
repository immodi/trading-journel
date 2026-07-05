using Application.Interfaces.Repositories;
using Domain.Entities;

namespace Infrastructure.Persistence.Repositories;

public class TradeRepository(AppDbContext context) : Repository<Trade>(context), ITradeRepository
{
    
}