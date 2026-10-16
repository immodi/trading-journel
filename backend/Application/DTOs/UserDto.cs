namespace Application.DTOs;

public class UserDto
{
    public int Id { get; set; }
    
    public required string Username { get; set; }
    public required string Email { get; set; }
    
    public DateTime CreatedAt { get; set; } 
    
    public ICollection<TradeDto> Trades { get; set; } = [];
}