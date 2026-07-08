using System.ComponentModel.DataAnnotations;
using Domain.Enum;

namespace Domain.Entities;

public class Trade
{
    public int Id { get; set; }
    
    public int UserId { get; set; }

    public User User { get; set; }

    public string Instrument { get; set; } = string.Empty;
    
    public Direction Direction { get; set; }
    
    public decimal EntryPrice { get; set; }
    public decimal ExitPrice { get; set; }
    public int Quantity { get; set; }

    public DateTime EntryTime { get; set; }
    public DateTime ExitTime { get; set; } 
    
    public decimal Commission { get; set; }
    public decimal ProfitLoss { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
