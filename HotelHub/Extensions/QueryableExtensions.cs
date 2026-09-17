using HotelHub.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelHub.API.Extensions
{
	public static class QueryableExtensions
	{
		public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
			this IQueryable<T> query,
			int pageNumber,
			int pageSize)
		{
			// Prevent invalid pagination values.
			pageNumber = Math.Max(pageNumber, 1);
			pageSize = Math.Clamp(pageSize, 1, 100);

			// Get the total number of records before pagination.
			var totalCount = await query.CountAsync();

			// Calculate how many records should be skipped.
			var skip = (pageNumber - 1) * pageSize;

			// Apply pagination and execute the query.
			var items = await query
				.Skip(skip)
				.Take(pageSize)
				.ToListAsync();

			return new PagedResult<T>
			{
				Items = items,
				PageNumber = pageNumber,
				PageSize = pageSize,
				TotalCount = totalCount
			};
		}
	}
}