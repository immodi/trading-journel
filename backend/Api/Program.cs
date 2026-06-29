using Api.Extensions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddAuthorization();
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

        builder.Logging.AddConsole();
        builder.Logging.AddDebug();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }
        
        app.ApplyMigrations();
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.UseDefaultFiles();
        app.UseStaticFiles(); 
        
        app.MapControllers();
     
        app.Run();
    }
    
    
        
}