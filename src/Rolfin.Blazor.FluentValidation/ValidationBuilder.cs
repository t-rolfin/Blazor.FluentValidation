namespace Rolfin.Blazor.FluentValidation;

public class ValidationBuilder<T>
{
    List<Validator<object>> _validators = new();
    RulesBuilder<T> _rules;


    ValidationBuilder(RulesBuilder<T> rules)
    {
        _rules = rules;
        _rules.Invoke(this);
    }

    internal Actions AddRule(Expression<Func<T, object>> property, bool removeIfRule)
    {
        var memberInfo = property.Body switch
        {
            UnaryExpression unary => (unary.Operand as MemberExpression).Member,
            MemberExpression member => member.Member,
            _ => throw new NotImplementedException()
        };

        var actions = new Actions();

        if (_validators.Any(x => x.Name == memberInfo.Name) is false)
        {
            var validator = new PropertyValidator(memberInfo.Name, string.Empty, actions.Filters, null, property.Compile());
            _validators.Add(validator);
        }
        else
        {
            var validator = _validators.FirstOrDefault(x => x.Name.Equals(memberInfo.Name));
            _validators.Remove(validator);
            actions.Filters = validator.Filters;

            if (validator.If is not null && removeIfRule is true) validator = validator with { If = null };
            _validators.Add(validator);
        }

        return actions;
    }


    public Actions RulesFor(Expression<Func<T, object>> property)
    {
        var memberInfo = property.Body switch
        {
            UnaryExpression unary => (unary.Operand as MemberExpression).Member,
            MemberExpression member => member.Member,
            _ => throw new NotImplementedException()
        };

        var actions = new Actions();

        if (_validators.Any(x => x.Name == memberInfo.Name) is false)
        {
            var validator = new PropertyValidator(memberInfo.Name, string.Empty, actions.Filters, null, property.Compile());
            _validators.Add(validator);
        }

        return actions;
    }
    public Actions RulesIfFor(Expression<Func<T, bool>> ifRule, Expression<Func<T, object>> property)
    {
        var memberInfo = property.Body switch
        {
            UnaryExpression unary => (unary.Operand as MemberExpression).Member,
            MemberExpression member => member.Member,
            _ => throw new NotImplementedException()
        };

        var actions = new Actions();

        if (_validators.Any(x => x.Name == memberInfo.Name) is false)
        {
            var validator = new PropertyValidator(memberInfo.Name, string.Empty, actions.Filters, ifRule.Compile(), property.Compile());
            _validators.Add(validator);
        }

        return actions;
    }


    public Actions RulesForRow<V>(Func<List<V>> list, Expression<Func<V, string>> rowIdentifier, Expression<Func<V, object>> property) where V : class
	{
        var memberInfo = property.Body switch
        {
            UnaryExpression unary => (unary.Operand as MemberExpression).Member,
            MemberExpression member => member.Member,
            _ => throw new NotImplementedException()
        };

        var items = list.Invoke();
		var actions = new Actions();

		if (items.Count <= 0) return actions;

		foreach (var item in items)
		{
			var rowIdentifierValue = rowIdentifier.Compile().Invoke(item);

			if (_validators.Any(x => x.Name == memberInfo.Name && x.Identifier == rowIdentifierValue) is false)
			{
                var validator = new ListValidator(
                    memberInfo.Name, rowIdentifierValue, actions.Filters, 
                    null, property.Compile(), list.ToDelegate<T, V>(), rowIdentifier.Compile());

				_validators.Add(validator);
			}
		}
        return actions;
    }
    public Actions RulesIfForRow<V>(Func<List<V>> list, Expression<Func<V, string>> rowIdentifier, Expression<Func<V, bool>> ifRule, Expression<Func<V, object>> property)
    {
        var memberInfo = property.Body switch
        {
            UnaryExpression unary => (unary.Operand as MemberExpression).Member,
            MemberExpression member => member.Member,
            _ => throw new NotImplementedException()
        };

        var items = list.Invoke();
		var actions = new Actions();

		if (items.Count <= 0) return actions;

		foreach (var item in items)
		{
			var rowIdentifierValue = rowIdentifier.Compile().Invoke(item);

            if (_validators.Any(x => x.Name == memberInfo.Name && x.Identifier == rowIdentifierValue) is false)
            {
				var validator = new ListValidator(
                    memberInfo.Name, rowIdentifierValue, actions.Filters, ifRule.Compile(), 
                    property.Compile(), list.ToDelegate<T, V>(), rowIdentifier.Compile());

				_validators.Add(validator);
			}
		}
		return actions;
	}


    public void Validate(ValidationMessageStore store, EditContext context)
    {
        _rules.Invoke(this);

        foreach (var validator in _validators)
        {
            var fieldIdentifier = context.Field(validator.Name);
            store.Clear(fieldIdentifier);

            if (validator.Filters.Any() is false) continue;

            switch(validator)
            {
                case PropertyValidator:
                    var value = validator.ValueHandler.DynamicInvoke(context.Model);
                    validator.Filters.Execute(fieldIdentifier, value, store);
                    break;

                case ListValidator converted:
                    var rowIdentifier = context.Field($"{converted.Identifier}.{converted.Name}");
                    store.Clear(rowIdentifier);
                    var items = converted.ListIdentifier.Compile().Invoke(context.Model);
                    foreach (var item in items)
                    {
                        var id = converted.RowIdentifier.DynamicInvoke(item);
                        if (id.Equals(converted.Identifier) is false) continue;
                        if (validator.If is not null && validator.If.DynamicInvoke(item) is false) continue;
                        var rowValue = converted.ValueHandler.DynamicInvoke(item);
						validator.Filters.Execute(rowIdentifier, rowValue, store);
					}

					break;
			};
        }
    }
    public void ValidateField(ValidationMessageStore store, EditContext context, FieldChangedEventArgs @event)
    {
        var validator = _validators.FirstOrDefault(x => x.Name.Equals(@event.FieldIdentifier.FieldName));
        if (validator is null) return;

        if (validator.Filters.Any() is false) return;

        store.Clear(@event.FieldIdentifier);
        switch (validator)
        {
            case PropertyValidator:
                var value = validator.ValueHandler.DynamicInvoke(@event.FieldIdentifier.Model);
                validator.Filters.Execute(@event.FieldIdentifier, value, store);
                break;
            case ListValidator converted:
                var eventId = converted.RowIdentifier.DynamicInvoke(@event.FieldIdentifier.Model);
                var rowIdentifier = context.Field($"{eventId}.{converted.Name}");
                store.Clear(rowIdentifier);
                var items = converted.ListIdentifier.Compile().Invoke(context.Model);
                foreach (var item in items)
                {
                    var id = converted.RowIdentifier.DynamicInvoke(item);
                    if (id.Equals(eventId) is false) continue;
                    if (validator.If is not null && validator.If.DynamicInvoke(item) is false) continue;
                    var rowValue = converted.ValueHandler.DynamicInvoke(item);
                    validator.Filters.Execute(rowIdentifier, rowValue, store);
                }
                break;
        };
    }


    public void Join(ValidationBuilder<T> anotherBuilder)
    {
        if (anotherBuilder == null) return;
        this._validators.AddRange(anotherBuilder._validators);
    }


    public static ValidationBuilder<T> Create(RulesBuilder<T> rules) => new ValidationBuilder<T>(rules);
}


