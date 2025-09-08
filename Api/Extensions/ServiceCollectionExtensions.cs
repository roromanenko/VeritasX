using Api.Mapping;
using Core.Interfaces;
using Core.Options;
using Infrastructure.Interfaces;
using Infrastructure.Jobs;
using Infrastructure.Mapping.Profiles;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.MongoDb;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Providers;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.Text;

namespace Api.Extensions;

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
		services.AddHttpClient();

		return services;
	}

	private static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<DataCollectorOptions>(configuration.GetSection(nameof(DataCollectorOptions)));

		services.AddScoped<IDataCollectionService, DataCollectionService>();
		services.AddScoped<ICandleChunkService, CandleChunkService>();
		services.AddHostedService<DataCollectorBackgroundService>();
		services.AddHostedService<TradingBotsJob>();
		services.AddHostedService<DatabaseCleanupJob>();
		services.AddScoped<IUserService, UserService>();
		services.AddScoped<IJwtService, JwtService>();
		services.AddScoped<PasswordHasher<UserEntity>>();
		services.AddAutoMapper(cfg =>
		{
			cfg.AddProfile<CandleProfile>();
			cfg.AddProfile<CandleChunkProfile>();
			cfg.AddProfile<DataChunkProfile>();
			cfg.AddProfile<DataCollectionJobProfile>();
			cfg.AddProfile<UserProfile>();
			cfg.AddProfile<CandleDtoProfile>();
			cfg.AddProfile<UserDtoProfile>();
			cfg.AddProfile<DataCollectionJobDtoProfile>();
		});

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
		services.AddScoped<ISymbolResolver, BinanceSymbolResolver>();

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
