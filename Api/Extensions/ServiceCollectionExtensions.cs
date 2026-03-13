using System.Text;
using System.Text.Json.Serialization;
using Api.Mapping;
using Core.Interfaces;
using Core.Options;
using Infrastructure.Exchanges;
using Infrastructure.Exchanges.Binance.Factory;
using Infrastructure.Exchanges.Binance.Services;
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
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using VeritasX.Api.Extensions;

namespace Api.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddVeritasxServices(this IServiceCollection services, IConfiguration configuration)
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
		services.Configure<EncryptionOptions>(configuration.GetSection(nameof(EncryptionOptions)));

		var masterKey = configuration["MASTER_ENCRYPTION_KEY"]
			?? throw new InvalidOperationException("MASTER_ENCRYPTION_KEY is not configured");

		services.AddScoped<IDataCollectionService, DataCollectionService>();
		services.AddScoped<ICandleChunkService, CandleChunkService>();
		services.AddHostedService<DataCollectorBackgroundService>();
		services.AddHostedService<TradingBotsJob>();
		services.AddHostedService<DatabaseCleanupJob>();
		services.AddScoped<IUserService, UserService>();
		services.AddScoped<IJwtService, JwtService>();
		services.AddScoped<IEncryptionService>(sp => new EncryptionService(masterKey, sp.GetRequiredService<IOptions<EncryptionOptions>>()));
		services.AddScoped<PasswordHasher<UserEntity>>();
		services.AddAutoMapper(cfg =>
		{
			cfg.AddProfile<CandleProfile>();
			cfg.AddProfile<CandleChunkProfile>();
			cfg.AddProfile<DataChunkProfile>();
			cfg.AddProfile<DataCollectionJobProfile>();
			cfg.AddProfile<UserProfile>();
			cfg.AddProfile<ExchangeDtoProfile>();
			cfg.AddProfile<UserDtoProfile>();
			cfg.AddProfile<DataCollectionJobDtoProfile>();
			cfg.AddProfile<BinanceProfile>();
			cfg.AddProfile<ExchangeConnectionDtoProfile>();
		});

		services.AddSignalR()
			.AddJsonProtocol(options =>
			{
				options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
				options.PayloadSerializerOptions.Converters.Add(new TimeSpanConverter());
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
		services.AddScoped<ITradeRepository, TradeRepository>();
		services.AddScoped<IDatabaseCleanupRepository, DatabaseCleanupRepository>();

		return services;
	}

	private static IServiceCollection AddBusinessServices(this IServiceCollection services)
	{
		services.AddScoped<IPriceProvider, BinancePriceProvider>();
		services.AddScoped<ISymbolResolver, BinanceSymbolResolver>();
		services.AddScoped<IBinanceClientFactory, BinanceClientFactory>();
		services.AddScoped<BinanceService>();
		services.AddScoped<IExchangeServiceFactory, ExchangeServiceFactory>();
		services.AddSingleton<IMarketDataStreamFactory, MarketDataStreamFactory>();

		services.AddBinance();

		return services;
	}

	private static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
		var jwtOptions = configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();

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
