namespace Application.DTOs;

public class UserDto
{
    public int Id { get; set; }
    
    public string Username { get; set; }
    public string Email { get; set; }
    
    public DateTime CreatedAt { get; set; } 
    
    public ICollection<TradeDto> Trades { get; set; } 
}