using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace HotelHub.UnitTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		Environment.SetEnvironmentVariable(
			"Jwt__Key",
			"ThisIsATestSecretKeyAtLeast32CharactersLong!");

		Environment.SetEnvironmentVariable(
			"Jwt__Issuer",
			"HotelHub.Tests");

		Environment.SetEnvironmentVariable(
			"Jwt__Audience",
			"HotelHub.Tests");

		Environment.SetEnvironmentVariable(
			"ConnectionStrings__DefaultConnection",
			"Server=(localdb)\\MSSQLLocalDB;Database=HotelHubTests;Trusted_Connection=True;");

		Environment.SetEnvironmentVariable(
			"ConnectionStrings__Redis",
			"localhost:6379,abortConnect=false");

		builder.UseEnvironment("Testing");

		builder.ConfigureAppConfiguration((context, config) =>
		{
			config.AddInMemoryCollection(new Dictionary<string, string?>
			{
				["Jwt:Key"] =
					"ThisIsATestSecretKeyAtLeast32CharactersLong!",
				["Jwt:Issuer"] = "HotelHub.Tests",
				["Jwt:Audience"] = "HotelHub.Tests",
				["ConnectionStrings:DefaultConnection"] =
					"Server=(localdb)\\MSSQLLocalDB;Database=HotelHubTests;Trusted_Connection=True;",
				["ConnectionStrings:Redis"] =
					"localhost:6379,abortConnect=false"
			});
		});
	}
}