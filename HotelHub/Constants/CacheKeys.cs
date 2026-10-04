using HotelHub.API.DTOs;
using HotelHub.API.Enums;

namespace HotelHub.API.Constants
{
	public static class CacheKeys
	{
		public static string Country(Guid id)
			=> $"country:{id}";

		public static string Countries()
			=> "Countries";

		public static string Hotel(Guid id)
			=> $"hotel:{id}";

		public static string CountriesList(
				string? searchTerm,
				SortingRequest? sorting,
				int pageNumber,
				int pageSize)
		{
			var search = string.IsNullOrWhiteSpace(searchTerm)
				? "none"
				: searchTerm.Trim().ToLowerInvariant();

			var sort = string.IsNullOrWhiteSpace(sorting?.SortBy)
				? "name"
				: sorting.SortBy.Trim().ToLowerInvariant();

			var direction =
				sorting is null ||
				string.IsNullOrWhiteSpace(sorting.SortBy)
					? SortDirection.Ascending
					: sorting.SortDirection;

			return $"countries:list:" +
				   $"search={search}:" +
				   $"sort={sort}:" +
				   $"dir={direction}:" +
				   $"page={pageNumber}:" +
				   $"size={pageSize}";
		}
	}
}
