using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace MockQueryable
{
	public class TestAsyncEnumerable<T> : TestQueryProvider<T>, IAsyncEnumerable<T>, IAsyncQueryProvider
	{
		public TestAsyncEnumerable(Expression expression)
			: base(expression)
		{
		}

		public TestAsyncEnumerable(IEnumerable<T> enumerable)
			: base(enumerable)
		{
		}

		protected override IQueryable<TEntity> CreateInstanceFromExpression<TEntity>(Expression expression)
			=> new TestAsyncEnumerable<TEntity>(expression);

		public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
			=> new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

		public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
		{
			var expectedResultType = typeof(TResult).GetGenericArguments()[0];
			var executionResult = typeof(IQueryProvider)
				.GetMethods()
				.First(method => method.Name == nameof(IQueryProvider.Execute) && method.IsGenericMethod)
				.MakeGenericMethod(expectedResultType)
				.Invoke(this, new[] {expression});

			return (TResult) typeof(Task).GetMethod(nameof(Task.FromResult))
				.MakeGenericMethod(expectedResultType)
				.Invoke(null, new[] {executionResult});
		}
	}
}