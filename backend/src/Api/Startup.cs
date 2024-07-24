using System.Text.Json.Serialization;
using Carter;
using Fei.Is.Api.Common.OpenAPI;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.Auth;
using Fei.Is.Api.Features.DataPoints.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Json;

namespace Fei.Is.Api;

public class Startup(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        // Add services to the container.
        services.ConfigurePostgresContext(Configuration);
        services.ConfigureTimescaleContext(Configuration);
        services.AddControllers();
        services.ConfigureProblemDetails();
        services.AddCarter();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Startup>());

        // Auth
        services.AddAuthentication();
        services.AddAuthorizationBuilder().SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
        services.ConfigureIdentity();
        services.ConfigureJWT(Configuration);

        services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds(type => type.FullName.Replace("+", "."));
            options.SupportNonNullableReferenceTypes();
            options.SchemaFilter<RequiredNotNullableSchemaFilter>();
        });

        // Add services
        services.AddScoped<TokenService>();
        services.AddSingleton<DataPointsBatchService>();
        services.AddHostedService(serviceProvider => serviceProvider.GetRequiredService<DataPointsBatchService>());

        // Add validators
        services.AddValidatorsFromAssemblyContaining<Startup>(ServiceLifetime.Scoped);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseDeveloperExceptionPage();
        }

        app.UseStatusCodePages();

        app.UseExceptionHandler();

        app.UsePathBase("/api");

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapCarter();
        });
    }
}
