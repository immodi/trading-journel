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
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            // Resolve content root (appsettings + wwwroot) next to the binary
            // rather than the current working directory, so the published,
            // self-contained app can be run from anywhere.
            ContentRootPath = AppContext.BaseDirectory,
        });

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("frontend", policy =>
            {
                policy
                    .WithOrigins("http://localhost:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        builder.Services.AddAuthorization();
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddJwtAuthentication(builder.Configuration);

        builder.Logging.AddConsole();
        builder.Logging.AddDebug();

        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<ExceptionHandler>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseExceptionHandler();

        app.ApplyMigrations();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseCors("frontend");

        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        // Unknown API routes should return JSON, not the SPA shell.
        app.MapFallback("/api/{**slug}", () =>
            Results.NotFound(new Application.Responses.ErrorResponse(
                404, "The requested API endpoint was not found.")));

        // Serve the React single-page app for client-side routes and
        // deep links that do not match an API endpoint (production build).
        // Guarded so a source-only (no `pnpm build`) run still starts cleanly.
        var webRoot = app.Environment.WebRootPath;
        if (!string.IsNullOrEmpty(webRoot) && File.Exists(Path.Combine(webRoot, "index.html")))
        {
            app.MapFallbackToFile("index.html");
        }

        app.Run();
    }



}
