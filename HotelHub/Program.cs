using System.Text;
using FluentValidation;
using HotelHub.API.Data;
using HotelHub.API.Exceptions;
using HotelHub.API.Models;
using HotelHub.API.Models.Auth;
using HotelHub.API.Services;
using HotelHub.API.Services.IServices;
using HotelHub.API.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
	.CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog((services, loggerConfiguration) => 
{
	loggerConfiguration
		.ReadFrom.Configuration(builder.Configuration)
		.ReadFrom.Services(services)
		.Enrich.FromLogContext();
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddHttpLogging(options =>
{
	options.LoggingFields =
		HttpLoggingFields.RequestMethod
		| HttpLoggingFields.RequestPath
		| HttpLoggingFields.ResponseStatusCode
		| HttpLoggingFields.Duration;
});

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddStackExchangeRedisOutputCache(options =>
{
	options.Configuration =
		builder.Configuration.GetConnectionString("Redis");

	options.InstanceName = "HotelHub:";
});

builder.Services.AddOutputCache();

builder.Services.AddValidatorsFromAssemblyContaining<CreateCountryDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateCountryDtoValidator>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi("v1", opts =>
{
	opts.AddDocumentTransformer((document, context, CancellationToken) =>
	{
		document.Info = new()
		{
			Title = "HotelHub API",
			Version = context.DocumentName,
			Description = "A hotel management and booking API for managing countries, hotels, and room reservations."
		};

		document.Components ??= new OpenApiComponents();
		document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
		document.Components.SecuritySchemes.Add("Bearer", new OpenApiSecurityScheme
		{
			Type = SecuritySchemeType.Http,
			Scheme = "bearer",
			BearerFormat = "JWT",
			Description = "Enter your Bearer token to access protected endpoints."
		});

		document.Security = [
			new OpenApiSecurityRequirement
			{
				{
					new OpenApiSecuritySchemeReference("Bearer"),
					[]
				}
			}
		];

		return Task.CompletedTask;
	});
});

builder.Services.AddAutoMapper(cfg =>
{
	cfg.AddMaps(typeof(Program).Assembly);
});

builder.Services.AddDbContext<ApplicationDbContext>(opts =>
{
	opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddHealthChecks()
	.AddDbContextCheck<ApplicationDbContext>();

builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<IHotelService, HotelService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IRoomService, RoomService>();

builder.Services.Configure<EmailOptions>(
	builder.Configuration.GetSection("Email"));

builder.Services.AddScoped<IEmailService, EmailService>();

// Identity
builder.Services.AddIdentityCore<ApplicationUser>()
				.AddRoles<IdentityRole<Guid>>()
				.AddEntityFrameworkStores<ApplicationDbContext>()
				.AddDefaultTokenProviders();

// Identity options
builder.Services.Configure<IdentityOptions>(opts =>
{
	opts.User.RequireUniqueEmail = true;

	opts.Password.RequiredLength = 8;
	opts.Password.RequireUppercase = true;
	opts.Password.RequireLowercase = true;
	opts.Password.RequireDigit = true;
	opts.Password.RequireNonAlphanumeric = true;
});

// JWT
var jwtKey = builder.Configuration["Jwt:Key"]
		?? throw new InvalidOperationException("JWT Key is not configured.");

var key = new SymmetricSecurityKey(
	Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddRateLimiter(options =>
{
	options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

	options.OnRejected = async (context, cancellationToken) =>
	{
		context.HttpContext.Response.ContentType =
			"application/json";

		await context.HttpContext.Response.WriteAsJsonAsync(
			new
			{
				Success = false,
				StatusCode = 429,
				Message = "Too many requests. Please try again later.",
				Data = (object?)null,
				Errors = (object?)null,
				TimeStamp = DateTime.UtcNow
			},
			cancellationToken);
	};

	// General API policy
	options.AddFixedWindowLimiter("api", limiterOptions =>
	{
		limiterOptions.PermitLimit = 100;
		limiterOptions.Window = TimeSpan.FromMinutes(1);
		limiterOptions.QueueLimit = 0;
	});

	// Authentication policy
	options.AddFixedWindowLimiter("auth", limiterOptions =>
	{
		limiterOptions.PermitLimit = 10;
		limiterOptions.Window = TimeSpan.FromMinutes(1);
		limiterOptions.QueueLimit = 0;
	});
});

builder.Services.AddAuthentication(opts =>
{
	opts.DefaultAuthenticateScheme =
		JwtBearerDefaults.AuthenticationScheme;

	opts.DefaultChallengeScheme =
		JwtBearerDefaults.AuthenticationScheme;

	opts.DefaultScheme =
		JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(opts =>
{
	opts.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidIssuer = builder.Configuration["Jwt:Issuer"],

		ValidateAudience = true,
		ValidAudience = builder.Configuration["Jwt:Audience"],

		ValidateIssuerSigningKey = true,
		IssuerSigningKey = key,

		ValidateLifetime = true,
		ClockSkew = TimeSpan.Zero
	};
});

builder.Services.AddAuthorizationBuilder()
	.AddPolicy("AdminOnly", policy =>
	{
		policy.RequireRole("Admin");
	})
	.AddPolicy("UserOnly", policy =>
	{
		policy.RequireRole("User");
	})
	.AddPolicy("AuthenticatedUser", policy =>
	{
		policy.RequireAuthenticatedUser();
	})
	.SetFallbackPolicy(new AuthorizationPolicyBuilder()
		.RequireAuthenticatedUser()
		.Build());


var app = builder.Build();

app.UseExceptionHandler();

using (var scope = app.Services.CreateScope())
{
	var roleManager = scope.ServiceProvider
		.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

	var userManager = scope.ServiceProvider
		.GetRequiredService<UserManager<ApplicationUser>>();

	await IdentitySeeder.SeedRoleAsync(roleManager);

	await IdentitySeeder.SeedAdminAsync(
		userManager,
		builder.Configuration);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi()
		.AllowAnonymous();

	app.MapScalarApiReference(opts =>
	{
		opts.WithTitle("HotelHub API")
			.WithDefaultHttpClient(
				ScalarTarget.CSharp,
				ScalarClient.HttpClient)
			.WithTheme(ScalarTheme.Solarized);

	})
	.AllowAnonymous();
}

app.UseHttpsRedirection();

app.UseSerilogRequestLogging();
app.UseHttpLogging();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.UseOutputCache();

app.MapHealthChecks("/health")
	.AllowAnonymous();

app.MapControllers();

try
{
	app.Run();
}
catch (Exception ex)
{
	Log.Fatal(ex, "Application terminated unexpectedly.");
}
finally
{
	Log.CloseAndFlush();
}