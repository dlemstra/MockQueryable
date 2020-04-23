using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MockQueryable
{
	public abstract class TestQueryProvider<T> : IOrderedQueryable<T>, IQueryProvider
	{
		private IEnumerable<T> _enumerable;

		public TestQueryProvider(Expression expression)
		{
			this.Expression = expression;
			this._enumerable = CompileExpressionItem<IEnumerable<T>>(this.Expression);
		}

		public TestQueryProvider(IEnumerable<T> enumerable)
		{
			this._enumerable = enumerable;
			this.Expression = enumerable.AsQueryable().Expression;
		}

		public Type ElementType => typeof(T);

		public Expression Expression { get; }

		public IQueryProvider Provider => this;

		public IQueryable CreateQuery(Expression expression)
			=> this.CreateInstanceFromExpression<T>(expression);

		public IQueryable<TEntity> CreateQuery<TEntity>(Expression expression)
			=> this.CreateInstanceFromExpression<TEntity>(expression);

		public object Execute(Expression expression)
			=> CompileExpressionItem<object>(expression);

		public TResult Execute<TResult>(Expression expression)
			=> CompileExpressionItem<TResult>(expression);

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
			=> this._enumerable.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
			=> this._enumerable.GetEnumerator();

		protected abstract IQueryable<TEntity> CreateInstanceFromExpression<TEntity>(Expression expression);

		private static TResult CompileExpressionItem<TResult>(Expression expression)
		{
			var rewriter = new TestExpressionVisitor();
			var body = rewriter.Visit(expression);
			var f = Expression.Lambda<Func<TResult>>(body, (IEnumerable<ParameterExpression>)null);
			return f.Compile()();
		}
	}
}