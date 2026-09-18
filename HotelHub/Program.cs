using System.Text;
using FluentValidation;
using HotelHub.API.Data;
using HotelHub.API.Models;
using HotelHub.API.Models.Auth;
using HotelHub.API.Services;
using HotelHub.API.Services.IServices;
using HotelHub.API.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

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
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));


builder.Services.AddAuthentication(opts => 
{
	opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	opts.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
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

app.UseOutputCache();

using (var scope = app.Services.CreateScope())
{
	var roleManager = scope.ServiceProvider
				.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

	var userManager = scope.ServiceProvider
					.GetRequiredService<UserManager<ApplicationUser>>();

	await IdentitySeeder.SeedRoleAsync(roleManager);

	await IdentitySeeder.SeedAdminAsync(
		userManager, builder.Configuration
	);
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
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
