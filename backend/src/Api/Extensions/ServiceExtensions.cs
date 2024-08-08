using System.Diagnostics;
using System.Text;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Data.Contexts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Fei.Is.Api.Extensions;

public static class ServiceExtensions
{
    public static void ConfigurePostgresContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgresConnection");
        services.AddDbContextPool<AppDbContext>(options => options.UseNpgsql(connectionString));
    }

    public static void ConfigureTimescaleContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("TimeScaleConnection");
        services.AddDbContextPool<TimeScaleDbContext>(options => options.UseNpgsql(connectionString));
    }

    public static void ConfigureIdentity(this IServiceCollection services)
    {
        var builder = services
            .AddIdentity<ApplicationUser, ApplicationRole>(o =>
            {
                // Password settings.
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequireUppercase = false;
                o.Password.RequiredLength = 6;
                o.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                o.Lockout.MaxFailedAccessAttempts = 10;
                o.Lockout.AllowedForNewUsers = true;

                // User settings.
                o.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                o.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
    }

    public static void ConfigureJWT(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["secret"] ?? throw new ArgumentException("JWT secret key is missing.");
        services
            .AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["validIssuer"],
                    ValidAudience = jwtSettings["validAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });
    }

    public static void ConfigureProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(
            options =>
                options.CustomizeProblemDetails = ctx =>
                {
                    if (!ctx.ProblemDetails.Extensions.ContainsKey("traceId"))
                    {
                        ctx.ProblemDetails.Extensions.Add("traceId", Activity.Current?.Id ?? ctx.HttpContext.TraceIdentifier);
                    }

                    ctx.ProblemDetails.Extensions.Add("instance", $"{ctx.HttpContext.Request.Method} {ctx.HttpContext.Request.Path}");
                }
        );
    }
}
