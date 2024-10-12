﻿namespace Rolfin.Blazor.FluentValidation;

static class Extensions
{
	public static Expression<Func<object, List<object>>> ToGeneric<T, V>(this Func<List<V>> property)
	{
		var returnConverted = (object x) =>
		{
			var returnResult = new List<object>();
			var listOf = property.Invoke();
			foreach (var item in listOf)
				returnResult.Add(item);

			return returnResult;
		};

		Expression<Func<object, List<object>>> res = (x) => returnConverted(x);

		return res;
	}
	public static Delegate ToDelegate<T>(this Expression<Func<T, object>> property)
	{
		var memberExpressionProp = property.Body as MemberExpression;
		var propertyInfo = property.Body is UnaryExpression uniaryExpresion is true
			? (uniaryExpresion.Operand as MemberExpression).Member as PropertyInfo
			: memberExpressionProp.Member as PropertyInfo;

		var parameterExpression = Expression.Parameter(typeof(T), "prop");
		var memberExpression = Expression.PropertyOrField(parameterExpression, propertyInfo.Name);
		var result = Expression.Lambda(memberExpression, parameterExpression).Compile();
		return result;
	}
	public static Delegate ToDelegate<T>(this Expression<Func<T, bool>> property)
	{
		var memberExpressionProp = property.Body as MemberExpression;
		var propertyInfo = property.Body is UnaryExpression uniaryExpresion is true
			? (uniaryExpresion.Operand as MemberExpression).Member as PropertyInfo
			: memberExpressionProp.Member as PropertyInfo;

		var parameterExpression = Expression.Parameter(typeof(T), "prop");
		var memberExpression = Expression.PropertyOrField(parameterExpression, propertyInfo.Name);
		var result = Expression.Lambda(memberExpression, parameterExpression).Compile();
		return result;
	}


	public static void Execute(this List<Filter> filters, FieldIdentifier fieldIdentifier, object value, ValidationMessageStore store)
	{
		foreach (var filter in filters)
		{
			var error = filter.Invoke(new(fieldIdentifier.FieldName, value));

			if (error != default)
			{
				store.Add(fieldIdentifier, error.Message);
				return;
			}

		}
	}
}