using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enum;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("/")]
public class IndexController(
    IWebHostEnvironment env, 
    ILogger<IndexController> logger,
    ITradeRepository tradeRepository,
    IUserRepository userRepository
    ) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        logger.LogInformation("Starting Main Application...");
        
        if (env.IsDevelopment())
        {
            var user = new User
            {
                Username = "johndoe",
                Email = "john.doe@example.com",
                PasswordHash = "AQAAAAEAACcQAAAAEExampleHashedPassword1234567890"
            };
            
            await userRepository.AddAsync(user);
            await userRepository.SaveChangesAsync();
            
            var trade = new Trade
            {
                UserId = 1,
                Instrument = "MNQ SEP26",
                Direction = Direction.Long,

                EntryPrice = 23456.25m,
                ExitPrice = 23468.75m,
                Quantity = 2,

                EntryTime = DateTime.UtcNow.AddMinutes(-15),
                ExitTime = DateTime.UtcNow,

                Commission = 4.20m,
                ProfitLoss = 245.80m
            };
            
            await tradeRepository.AddAsync(trade);
            await tradeRepository.SaveChangesAsync();


            return Ok("this is a development environment, run the frontend application separately.");
        }
        
        var path = Path.Combine(env.WebRootPath, "index.html");
        return PhysicalFile(path, "text/html");;
    }
}