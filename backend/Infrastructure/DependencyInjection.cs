using System.Security.Cryptography;
using System.Text;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using IJwtTokenService = Application.Interfaces.Services.IJwtTokenService;

namespace Infrastructure;

public static class DependencyInjection
{
    private const int MinimumSecretBytes = 32;

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Default")));

        services.AddScoped<ITradeRepository, TradeRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        
        services.AddScoped<ITradeService, TradeService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtTokenService>(_ => new JwtTokenService(configuration)); 

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Resolve the signing key up front so a misconfiguration fails fast at
        // startup instead of throwing on every request.
        var secret = ResolveJwtSecret(configuration, out var generated);

        // Expose the resolved key to the rest of the app (JwtTokenService signs
        // with the same configuration section).
        configuration["JwtSettings:Secret"] = secret;

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            // Keep the JWT claim names exactly as they were issued (see JwtClaimNames)
            // instead of letting the handler remap them to legacy WS-Federation URIs.
            options.MapInboundClaims = false;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["JwtSettings:Issuer"],
                ValidAudience = configuration["JwtSettings:Audience"],
                NameClaimType = JwtClaimNames.Name,
                RoleClaimType = JwtClaimNames.Role,
                IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
            };
        });

        if (generated)
        {
            Console.WriteLine(
                $"JwtSettings:Secret was not configured; generated a signing key at " +
                $"'{JwtSecretPath}'. Set the 'JwtSettings__Secret' environment variable " +
                "to use your own key.");
        }

        return services;
    }

    private static string JwtSecretPath => Path.Combine(AppContext.BaseDirectory, "jwt-secret.key");

    /// <summary>
    /// Returns the configured signing key, generating and persisting one next to
    /// the binary when none is supplied (self-contained deployments) so issued
    /// tokens remain valid across restarts.
    /// </summary>
    private static string ResolveJwtSecret(IConfiguration configuration, out bool generated)
    {
        generated = false;

        var configured = configuration["JwtSettings:Secret"];
        if (!string.IsNullOrWhiteSpace(configured))
        {
            if (Encoding.UTF8.GetByteCount(configured) < MinimumSecretBytes)
            {
                throw new InvalidOperationException(
                    $"JwtSettings:Secret must be at least {MinimumSecretBytes} bytes. " +
                    "Provide a stronger signing key via configuration or the " +
                    "'JwtSettings__Secret' environment variable.");
            }

            return configured;
        }

        var path = JwtSecretPath;
        try
        {
            if (File.Exists(path))
            {
                var existing = File.ReadAllText(path).Trim();
                if (Encoding.UTF8.GetByteCount(existing) >= MinimumSecretBytes)
                {
                    return existing;
                }
            }

            var created = Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(48));
            File.WriteAllText(path, created);
            generated = true;
            return created;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new InvalidOperationException(
                "JwtSettings:Secret is not configured and a signing key could not be " +
                $"persisted to '{path}'. Set a strong signing key via configuration or " +
                "the 'JwtSettings__Secret' environment variable.", ex);
        }
    }
}