using System.Text.Json;
using HotelHub.API.Services.IServices;
using Microsoft.Extensions.Caching.Distributed;

namespace HotelHub.API.Services
{
	public class RedisCacheService (IDistributedCache cache,
									ILogger<RedisCacheService> logger)
	: ICacheService
	{
		public async Task<T?> GetAsync<T>(
			string key, 
			CancellationToken cancellationToken = default)
		{
			var json = await cache.GetStringAsync(
							key,
							cancellationToken
			);

			if (json is null)
			{
				logger.LogInformation(
					"Cache MISS for key {key}",
					key
				);
				return default;
			}

			logger.LogInformation(
				"Cache HIT for key {key}",
				key
			);

			return JsonSerializer.Deserialize<T>(json);
		}

		public async Task RemoveAsync(
				string key, 
				CancellationToken cancellationToken = default)
		{
			await cache.RemoveAsync(
						key,
						cancellationToken
			);

			logger.LogInformation(
				"Cache REMOVE for key {Key}",
				key);
		}

		public async Task SetAsync<T>(
				string key, 
				T value, 
				TimeSpan expiration, 
				CancellationToken cancellationToken = default)
		{
			var json = JsonSerializer.Serialize(value);

			var option = new DistributedCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = expiration
			};

			await cache.SetStringAsync(
					key,
					json,
					option,
					cancellationToken
			);

			logger.LogInformation(
				"Cache SET for key {Key} with expiration {Expiration}",
				key,
				expiration);
		}
	}
}
