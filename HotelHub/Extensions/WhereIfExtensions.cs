using System.Linq.Expressions;

namespace HotelHub.API.Extensions
{
	public static class WhereIfExtensions
	{
		public static IQueryable<T> WhereIf<T>(
			this IQueryable<T> query,
			bool condition,
			Expression<Func<T, bool>> predicate)
		{
			return condition
				? query.Where(predicate)
				: query;
		}
	}
}