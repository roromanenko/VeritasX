using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VeritasX.Infrastructure.Providers;
using VeritasX.Core.Interfaces;
using VeritasX.Core.Options;
using VeritasX.Infrastructure.Persistence.MongoDb;
using MongoDB.Driver;
using VeritasX.Core.Domain;
using VeritasX.Application.Services;
using VeritasX.Infrastructure.Jobs;
using VeritasX.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Identity;
using VeritasX.Infrastructure.Persistence.Entities;

namespace VeritasX.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVeritasServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCachingServices(configuration);
        services.AddHttpServices();
        services.AddBusinessServices();
        services.AddApplicationServices(configuration);
        services.AddMongoDbServices(configuration);
        services.AddAuthenticationServices();
        
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder.WithOrigins("http://localhost:5173") // React dev server
                       .AllowAnyHeader()
                       .AllowAnyMethod()
                       .AllowCredentials(); // For cookies
            });
        });

        return services;
    }

    private static IServiceCollection AddCachingServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Регистрируем конфигурацию кэша
        services.Configure<CacheOptions>(configuration.GetSection(nameof(CacheOptions)));
        var cacheOptions = configuration.GetSection(nameof(CacheOptions)).Get<CacheOptions>() ?? new CacheOptions();
        
        services.AddMemoryCache(options =>
        {
            options.SizeLimit = cacheOptions.SizeLimit;
            options.CompactionPercentage = cacheOptions.CompactionPercentage;
        });

        return services;
    }

    private static IServiceCollection AddHttpServices(this IServiceCollection services)
    {
        services.AddHttpClient<BinancePriceProvider>(client =>
        {
            client.BaseAddress = new Uri("https://api.binance.com/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DataCollectorOptions>(configuration.GetSection(nameof(DataCollectorOptions)));
        services.AddScoped<IDataCollectionService, DataCollectionService>();
        services.AddScoped<ICandleChunkService, CandleChunkService>();
        services.AddHostedService<DataCollectorBackgroundService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<PasswordHasher<User>>();

        return services;
    }

    private static IServiceCollection AddMongoDbServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoDbOptions>(configuration.GetSection(nameof(MongoDbOptions)));
        
        services.AddScoped<IMongoClient>(sp =>
        {
            string connectionString = configuration.GetConnectionString("MongoDb")!;
            var settings = MongoClientSettings.FromConnectionString(connectionString);
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            var client = new MongoClient(settings);

            return client;
        });

        services.AddScoped<IMongoDbContext, MongoDbContext>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }

    private static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        services.AddScoped<IPriceProvider, BinancePriceProvider>();
        services.AddScoped<ICachedPriceProvider, CachedPriceProvider>();

        return services;
    }

        private static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
    {
        services.AddAuthentication("Cookies")
            .AddCookie("Cookies", options =>
            {
                options.LoginPath = "/api/user/login";
                options.LogoutPath = "/api/user/logout";
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                };
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
            });

        return services;
    }
}
