using FluentValidation;
using HotelHub.API.Data;
using HotelHub.API.Services;
using HotelHub.API.Services.IServices;
using HotelHub.API.Validators;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddValidatorsFromAssemblyContaining<CreateCountryDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateCountryDtoValidator>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.MapScalarApiReference(opts =>
	{
		opts.WithTitle("HotelHub API")
			.WithDefaultHttpClient(
				ScalarTarget.CSharp,
				ScalarClient.HttpClient)
			.WithTheme(ScalarTheme.Solarized);
	});
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
