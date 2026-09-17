using System.Linq.Expressions;

namespace HotelHub.API.Extensions
{
	public static class SearchExtensions
	{
		public static IQueryable<T> Search<T>(
			this IQueryable<T> query,
			string? searchTerm,
			params Expression<Func<T, string>>[] properties)
		{
			// If there is no search term, return the original query.
			if (string.IsNullOrWhiteSpace(searchTerm))
			{
				return query;
			}

			searchTerm = searchTerm.Trim();

			// Build:
			// property.Contains(searchTerm)
			//
			// for every property passed to the method.
			Expression? combinedExpression = null;

			var parameter = Expression.Parameter(
				typeof(T),
				"x");

			foreach (var property in properties)
			{
				// Replace the original parameter with our shared parameter.
				var propertyExpression =
					new ReplaceExpressionVisitor(
						property.Parameters[0],
						parameter)
					.Visit(property.Body);

				if (propertyExpression is null)
				{
					continue;
				}

				// x.Property
				var containsMethod = typeof(string)
					.GetMethod(
						nameof(string.Contains),
						new[] { typeof(string) })!;

				// x.Property.Contains(searchTerm)
				var containsExpression = Expression.Call(
					propertyExpression,
					containsMethod,
					Expression.Constant(searchTerm));

				combinedExpression =
					combinedExpression is null
						? containsExpression
						: Expression.OrElse(
							combinedExpression,
							containsExpression);
			}

			if (combinedExpression is null)
			{
				return query;
			}

			var lambda = Expression.Lambda<Func<T, bool>>(
				combinedExpression,
				parameter);

			return query.Where(lambda);
		}

		private sealed class ReplaceExpressionVisitor(
			Expression oldExpression,
			Expression newExpression)
			: ExpressionVisitor
		{
			protected override Expression VisitParameter(
				ParameterExpression node)
			{
				return node == oldExpression
					? newExpression
					: base.VisitParameter(node);
			}
		}
	}
}
