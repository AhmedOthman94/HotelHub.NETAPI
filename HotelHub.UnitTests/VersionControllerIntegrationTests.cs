using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace HotelHub.UnitTests;

public class VersionControllerIntegrationTests
{
	[Fact]
	public async Task GetV1_ReturnsSuccessAndExpectedHotelData()
	{
		await using var factory = new CustomWebApplicationFactory();
		using var client = factory.CreateClient();

		var response = await client.GetAsync(
			"/api/version?api-version=1.0");

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);

		var content = await response.Content.ReadAsStringAsync();

		Assert.Contains("Grand Hotel", content);
		Assert.Contains("perNight", content);
	}

	[Fact]
	public async Task GetV2_ReturnsSuccessAndExpectedHotelData()
	{
		await using var factory = new CustomWebApplicationFactory();
		using var client = factory.CreateClient();

		var response = await client.GetAsync(
			"/api/version?api-version=2.0");

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);

		var content = await response.Content.ReadAsStringAsync();

		Assert.Contains("Grand Hotel", content);
		Assert.Contains("rooms", content);
		Assert.Contains("roomType", content);
		Assert.Contains("pricePerNight", content);
	}

	[Fact]
	public async Task GetUnsupportedVersion_ReturnsClientError()
	{
		await using var factory = new CustomWebApplicationFactory();
		using var client = factory.CreateClient();

		var response = await client.GetAsync(
			"/api/version?api-version=3.0");

		var content = await response.Content.ReadAsStringAsync();

		Assert.True(
			(int)response.StatusCode >= 400 &&
			(int)response.StatusCode < 500,
			$"Expected a 4xx status code, but received {(int)response.StatusCode}. Response: {content}");

		Assert.NotEqual(
			System.Net.HttpStatusCode.Unauthorized,
			response.StatusCode);
	}
}
