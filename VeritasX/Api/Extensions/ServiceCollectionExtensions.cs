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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

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
		services.AddJwtAuthentication(configuration);

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
		services.AddHostedService<DatabaseCleanupJob>();
		services.AddScoped<IUserService, UserService>();
		services.AddScoped<IJwtService, JwtService>();
		services.AddScoped<PasswordHasher<User>>();
		services.AddAutoMapper(cfg => { }, typeof(Program).Assembly);

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
		services.AddScoped<IDatabaseCleanupRepository, DatabaseCleanupRepository>();

		return services;
	}

	private static IServiceCollection AddBusinessServices(this IServiceCollection services)
	{
		services.AddScoped<IPriceProvider, BinancePriceProvider>();
		services.AddScoped<ICachedPriceProvider, CachedPriceProvider>();

		return services;
	}

	private static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
	{
		// Регистрируем JWT опции
		services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
		var jwtOptions = configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();

		// Настраиваем JWT аутентификацию
		services.AddAuthentication(options =>
		{
			options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
		})
		.AddJwtBearer(options =>
		{
			options.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
				ValidateIssuer = true,
				ValidIssuer = jwtOptions.Issuer,
				ValidateAudience = true,
				ValidAudience = jwtOptions.Audience,
				ValidateLifetime = true,
				ClockSkew = TimeSpan.Zero,
			};
		});

		return services;
	}
}
