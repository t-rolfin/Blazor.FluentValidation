using System.Reflection;

namespace Rolfin.Blazor.FluentValidation;

public class ValidationBuilder<T>
{
    List<PropertyValidation<T>> _propertyValidators = new();
    EditContext _editContext;


    public Actions RoleFor(Expression<Func<T, object>> property)
    {

        var memberExpression = property.Body as MemberExpression;
        var propertyInfo = property.Body is UnaryExpression uniaryExpresion is true
            ? (uniaryExpresion.Operand as MemberExpression).Member as PropertyInfo
            : memberExpression.Member as PropertyInfo;


        var actions = new Actions();

        if (_propertyValidators.Any(x => x.Name == propertyInfo.Name) is false)
        {
            var validator = new PropertyValidation<T>(property, propertyInfo.Name);
            validator._filters = actions.Filters;

            _propertyValidators.Add(validator);
        }

        return actions;
    }
    public Actions RoleIfFor(Expression<Func<T, bool>> ifRole, Expression<Func<T, object>> property)
    {
        var memberExpression = property.Body as MemberExpression;
        var propertyInfo = memberExpression.Member as PropertyInfo;

        var actions = new Actions();

        if (_propertyValidators.Any(x => x.Name == propertyInfo.Name) is false)
        {
            var validator = new PropertyValidation<T>(property, propertyInfo.Name);
            validator._filters = actions.Filters;
            validator._if = ifRole;

            _propertyValidators.Add(validator);
        }

        return actions;
    }


    public void Validate(ValidationMessageStore store, EditContext context)
    {
        foreach(var validator in _propertyValidators)
        {
            var fieldIdentifier = context.Field(validator.Name);
            store.Clear(fieldIdentifier);

            if (validator._if is not null && validator._if.Compile().Invoke((T)context.Model) is false) continue;
            if (validator._filters.Any() is false) continue;

            var value = validator._property.Compile().Invoke((T)context.Model);
            ExecutFilters(validator._filters, fieldIdentifier, value, store);
        }
    }
    public void ValidateField(ValidationMessageStore store, EditContext context, FieldIdentifier fieldIdentifier)
    {
        var fieldValidator = _propertyValidators.FirstOrDefault(x => x.Name.Equals(fieldIdentifier.FieldName));
        if (fieldValidator is null) return;

        store.Clear(fieldIdentifier);
        var value = fieldValidator._property.Compile().Invoke((T)context.Model);
        ExecutFilters(fieldValidator._filters, fieldIdentifier, value, store);
    }


    void ExecutFilters(List<Func<FieldProperties, ValidationError>> filters, FieldIdentifier fieldIdentifier, object value, ValidationMessageStore store)
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