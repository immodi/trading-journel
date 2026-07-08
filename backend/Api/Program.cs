using Api.Exceptions;
using Api.Extensions;
using Infrastructure;
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
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddJwtAuthentication(builder.Configuration);
        
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();
        
        builder.Services.AddExceptionHandler<ExceptionHandler>();
        
        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }
        
        app.UseExceptionHandler();
        
        app.ApplyMigrations();
        app.UseHttpsRedirection();
        app.UseDefaultFiles();
        app.UseStaticFiles(); 
        
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
     
        app.Run();
    }
    
    
        
}