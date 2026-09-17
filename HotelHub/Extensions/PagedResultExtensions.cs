using AutoMapper;
using HotelHub.API.Models;

namespace HotelHub.API.Extensions
{
	public static class PagedResultExtensions
	{
		public static PagedResult<TDestination> MapTo<TSource, TDestination>(
			this PagedResult<TSource> source,
			IMapper mapper)
		{
			return new PagedResult<TDestination>
			{
				Items = mapper.Map<IEnumerable<TDestination>>(
					source.Items),

				PageNumber = source.PageNumber,

				PageSize = source.PageSize,

				TotalCount = source.TotalCount
			};
		}
	}
}
