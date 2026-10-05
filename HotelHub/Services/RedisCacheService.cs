using System.Text.Json;
using HotelHub.API.Services.IServices;
using Microsoft.Extensions.Caching.Distributed;
using Polly;
using Polly.Registry;

namespace HotelHub.API.Services
{
	public class RedisCacheService(
		IDistributedCache cache,
		ILogger<RedisCacheService> logger,
		ResiliencePipelineProvider<string> pipelineProvider)
		: ICacheService
	{
		private readonly ResiliencePipeline _redisPipeline =
			pipelineProvider.GetPipeline("redis-retry");

		public async Task<T?> GetAsync<T>(
			string key,
			CancellationToken cancellationToken = default)
		{
			try
			{
				var json = await _redisPipeline.ExecuteAsync(
					async token =>
						await cache.GetStringAsync(
							key,
							token),
					cancellationToken);

				if (json is null)
				{
					logger.LogInformation(
						"Cache MISS for key {Key}",
						key);

					return default;
				}

				logger.LogInformation(
					"Cache HIT for key {Key}",
					key);

				return JsonSerializer.Deserialize<T>(json);
			}
			catch (Exception ex)
			{
				logger.LogError(
					ex,
					"Redis GET failed for key {Key}",
					key);

				return default;
			}
		}

		public async Task RemoveAsync(
			string key,
			CancellationToken cancellationToken = default)
		{
			try
			{
				await cache.RemoveAsync(
					key,
					cancellationToken);

				logger.LogInformation(
					"Cache REMOVE for key {Key}",
					key);
			}
			catch (Exception ex)
			{
				logger.LogError(
					ex,
					"Redis REMOVE failed for key {Key}",
					key);
			}
		}

		public async Task SetAsync<T>(
			string key,
			T value,
			TimeSpan expiration,
			CancellationToken cancellationToken = default)
		{
			try
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
					cancellationToken);

				logger.LogInformation(
					"Cache SET for key {Key} with expiration {Expiration}",
					key,
					expiration);
			}
			catch (Exception ex)
			{
				logger.LogError(
					ex,
					"Redis SET failed for key {Key}",
					key);
			}
		}
	}
}