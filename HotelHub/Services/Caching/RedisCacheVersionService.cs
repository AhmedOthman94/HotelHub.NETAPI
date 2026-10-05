using StackExchange.Redis;

namespace HotelHub.API.Services.Caching
{
	public class RedisCacheVersionService(
		IConnectionMultiplexer redis,
		ILogger<RedisCacheVersionService> logger)
		: ICacheVersionService
	{
		private readonly IDatabase _database =
			redis.GetDatabase();

		public async Task<long> GetAsync(string key)
		{
			try
			{
				var value =
					await _database.StringGetAsync(key);

				if (value.HasValue)
				{
					return (long)value;
				}

				await _database.StringSetAsync(key, 1);

				return 1;
			}
			catch (RedisException ex)
			{
				logger.LogError(
					ex,
					"Redis GET version failed for key {Key}",
					key);

				return 0;
			}
		}

		public async Task<long> IncrementAsync(string key)
		{
			try
			{
				return await _database.StringIncrementAsync(key);
			}
			catch (RedisException ex)
			{
				logger.LogError(
					ex,
					"Redis INCREMENT failed for key {Key}",
					key);

				return 0;
			}
		}
	}
}