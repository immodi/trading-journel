using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Default"))
                .EnableSensitiveDataLogging()
                .LogTo(Console.WriteLine, LogLevel.Information));
        
        services.AddScoped<ITradeRepository, TradeRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<ITradeService, TradeService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        
        return services;
    }
}