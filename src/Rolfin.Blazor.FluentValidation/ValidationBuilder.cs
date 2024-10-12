namespace Rolfin.Blazor.FluentValidation;

delegate object Convert(object obj);

public class ValidationBuilder<T>
{
    List<Validator<object>> _validators = new();
    EditContext _editContext;

    public Actions RolesFor(Expression<Func<T, object>> property)
    {

        var memberExpression = property.Body as MemberExpression;
        var propertyInfo = property.Body is UnaryExpression uniaryExpresion is true
            ? (uniaryExpresion.Operand as MemberExpression).Member as PropertyInfo
            : memberExpression.Member as PropertyInfo;

        var actions = new Actions();

        if (_validators.Any(x => x.Name == propertyInfo.Name) is false)
        {
            var validator = new PropertyValidator(propertyInfo.Name, actions.Filters, null, property.ToDelegate());
            _validators.Add(validator);
        }

        return actions;
    }
    public Actions RolesIfFor(Expression<Func<T, bool>> ifRole, Expression<Func<T, object>> property)
    {
        var memberExpression = property.Body as MemberExpression;
        var propertyInfo = memberExpression.Member as PropertyInfo;

        var actions = new Actions();

        if (_validators.Any(x => x.Name == propertyInfo.Name) is false)
        {
            var validator = new PropertyValidator(propertyInfo.Name, actions.Filters, ifRole.ToDelegate(), property.ToDelegate());
            _validators.Add(validator);
        }

        return actions;
    }

    public Actions RolesForRow<V>(Func<List<V>> listIdentifier, Expression<Func<V, object>> rowIdentifier, Expression<Func<V, object>> property) where V : class
	{
		var memberExpression = property.Body as MemberExpression;
		var propertyInfo = property.Body is UnaryExpression uniaryExpresion is true
			? (uniaryExpresion.Operand as MemberExpression).Member as PropertyInfo
			: memberExpression.Member as PropertyInfo;


		var list = listIdentifier.Invoke();
		var actions = new Actions();

		if (list.Count <= 0) return actions;

		foreach (var item in list)
		{
			var rowIdentifierValue = rowIdentifier.Compile().Invoke(item);
			var name = $"{rowIdentifierValue}.{propertyInfo.Name}";

			if (_validators.Any(x => x.Name == name) is false)
			{
				var validator = new ListValidator(name, actions.Filters, null, property.ToDelegate(), listIdentifier.ToGeneric<T, V>());
				_validators.Add(validator);
			}
		}
        return actions;
    }

    public void Validate(ValidationMessageStore store, EditContext context)
    {
        foreach(var validator in _validators)
        {
            var fieldIdentifier = context.Field(validator.Name);
            store.Clear(fieldIdentifier);

            if (validator.If is not null && validator.If.DynamicInvoke(context.Model) is false) continue;
            if (validator.Filters.Any() is false) continue;

            switch(validator)
            {
                case PropertyValidator:
                    var value = validator.ValueHandler.DynamicInvoke((T)context.Model);
                    validator.Filters.Execute(fieldIdentifier, value, store);
                    break;
                case ListValidator converted:
                    var ss = converted.ListIdentifier.Compile().Invoke(context.Model);
                    foreach (var v in ss)
                    {
                        var rowValue = converted.ValueHandler.DynamicInvoke(v);
						validator.Filters.Execute(fieldIdentifier, rowValue, store);
					}
					break;
			};
        }
    }
    public void ValidateField(ValidationMessageStore store, EditContext context, FieldIdentifier fieldIdentifier)
    {
        var fieldValidator = _validators.FirstOrDefault(x => x.Name.Equals(fieldIdentifier.FieldName));
        if (fieldValidator is null) return;

        store.Clear(fieldIdentifier);
        var value = fieldValidator.ValueHandler.DynamicInvoke(context.Model);
        fieldValidator.Filters.Execute(fieldIdentifier, value, store);
    }
}


