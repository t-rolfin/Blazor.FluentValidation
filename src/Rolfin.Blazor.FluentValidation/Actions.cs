namespace Rolfin.Blazor.FluentValidation;

public class Actions
{
    internal List<Func<FieldProperties, ValidationError>> Filters = new();


    public Actions AddFilter(Func<FieldProperties, ValidationError> filter)
    {
        Filters.Add(filter);
        return this;
    }


    public Actions AddValidators(params Func<FieldProperties, ValidationError>[] validators)
    {
        for(var i = 0; i <= validators.Length - 1; i++)
            Filters.Add(validators[i]);

        return this;
    }
    public Actions NotNullOrWhiteSpace()
    {
        return AddFilter(x =>
        {
            if (x.Value is Guid && (Guid)x.Value == Guid.Empty) 
                return new(string.Empty, $"\"{x.Name}\" can't have defatul value.");

            if (x.Value is string str && string.IsNullOrWhiteSpace(str)) 
                return new(string.Empty, $"The value for field \"{x.Name}\" can't be null or whitespace.");

            if (x.Value is null) 
                return new(string.Empty, $"The value for field \"{x.Name}\" can't be null or whitesapce");

            return default;
        });
    }
    public Actions NotNull()
    {
        return AddFilter((x) =>
        {
            if (x.Value is Guid && (Guid)x.Value == Guid.Empty) return new(string.Empty, "Value can't be defatul.");
            if (x.Value is string && x.Value is null) return new(string.Empty, "Value can't be null");
            if (x.Value is null) return new(string.Empty, "Value can't be null.");

            return default;
        });
    }
}


public static class ValidatorDefaults
{
    public static ValidationError Exists(this IList items, FieldProperties field)
    {
        foreach (var item in items)
        {
            var property = item.GetType().GetProperty(field.Name);
            var value = property.GetValue(item, null);
            if (value.Equals(field.Value)) return new(string.Empty, $"A item with this value as \"{field.Name}\" already exists.");
        }

        return default;
    }          
}