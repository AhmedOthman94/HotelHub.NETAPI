using System.Linq.Expressions;
using System.Reflection;

namespace HotelHub.API.Extensions
{
	public static class SortingExtensions
	{
		public static IQueryable<T> OrderByProperty<T>(
			this IQueryable<T> query,
			string? propertyName,
			bool descending = false)
		{
			if (string.IsNullOrWhiteSpace(propertyName))
			{
				return query;
			}

			var property = typeof(T).GetProperty(
							propertyName,
							BindingFlags.Public |
							BindingFlags.Instance |
							BindingFlags.IgnoreCase);

			if (property is null)
			{
				return query;
			}

			var parameter = Expression.Parameter(
				typeof(T),
				"x");

			var propertyExpression = Expression.Property(
				parameter,
				property);

			var lambda = Expression.Lambda(
				propertyExpression,
				parameter);

			var methodName = descending
				? nameof(Queryable.OrderByDescending)
				: nameof(Queryable.OrderBy);

			var result = Expression.Call(
				typeof(Queryable),
				methodName,
				new[] { typeof(T), property.PropertyType },
				query.Expression,
				Expression.Quote(lambda));

			return query.Provider.CreateQuery<T>(result);
		}
	}
}