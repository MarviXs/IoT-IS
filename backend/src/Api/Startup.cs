using System.Text.Json.Serialization;
using Carter;
using Fei.Is.Api.BackgroundServices;
using Fei.Is.Api.Common.OpenAPI;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.Auth;
using Fei.Is.Api.Features.Products;
using Fei.Is.Api.Features.Jobs.Services;
using Fei.Is.Api.MqttClient;
using Fei.Is.Api.MqttClient.Publish;
using Fei.Is.Api.MqttClient.Subscribe;
using Fei.Is.Api.Redis;
using Fei.Is.Api.SignalR.Hubs;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Json;

namespace Fei.Is.Api;

public class Startup(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        bool isMqttEnabled = Configuration.GetValue<bool>("MqttSettings:Enabled");

        // Add services to the container.
        services.ConfigurePostgresContext(Configuration);
        services.ConfigureTimescaleContext(Configuration);
        services.ConfigureCors(Configuration);
        services.ConfigureProblemDetails();
        services.AddCarter();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Startup>());
        services.AddSignalR();

        // Auth
        services.AddAuthentication();
        services
            .AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())
            .AddPolicy("Admin", policy => policy.RequireRole("Admin"));
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
            options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
            options.SupportNonNullableReferenceTypes();
            options.SchemaFilter<RequiredNotNullableSchemaFilter>();
        });

        // Add services
        services.AddScoped<JobService>();
        services.AddScoped<TokenService>();
        services.AddSingleton<RedisService>();

        if (isMqttEnabled)
        {
            services.AddSingleton<MqttClientService>();
            services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<MqttClientService>());
        }

        services.AddHostedService<StoreDataPointsBatchService>();
        services.AddHostedService<JobTimeOutService>();

        services.AddScoped<PLUCodeService>();
        services.AddHostedService<SceneEvaluateService>();

        //MQTT Services
        services.AddScoped<JobStatusReceived>();
        services.AddScoped<OnDeviceDisconnected>();
        services.AddScoped<OnDeviceConnected>();
        services.AddScoped<DataPointReceived>();
        services.AddScoped<PublishJobControl>();
        services.AddScoped<PublishJobStatus>();

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

        app.UseCors("CorsPolicy");

        app.UseStatusCodePages();

        app.UseExceptionHandler();

        app.UsePathBase("/api");

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapCarter();
            endpoints.MapHub<IsHub>("/is-hub");
        });
    }
}
