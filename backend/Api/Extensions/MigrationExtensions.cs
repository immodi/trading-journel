using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Api.Extensions;

public static class MigrationExtensions
{
    public static WebApplication ApplyMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        dbContext.Database.Migrate();

        return app;
    }
}