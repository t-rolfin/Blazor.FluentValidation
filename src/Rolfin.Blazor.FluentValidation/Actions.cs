namespace Rolfin.Blazor.FluentValidation;

public class Actions
{
    internal List<Filter> Filters = new();


    public Actions AddValidators(params Filter[] validators)
    {
        for (var i = 0; i <= validators.Length - 1; i++)
            if (Filters.Any(x => x == validators[i]) is false)
                Filters.Add(validators[i]);

        return this;
    }
    public Actions AddValidator(Filter validator)
    {
        if(Filters.Any(x => x == validator) is false) Filters.Add(validator);
        return this;
    }
    public Actions NotNullOrWhiteSpace()
    {
        return AddValidator(x =>
        {
            if (x.Value is Guid && (Guid)x.Value == Guid.Empty) 
                return new(string.Empty, $"This field can't have defatul value.");

            if (x.Value is string str && string.IsNullOrWhiteSpace(str)) 
                return new(string.Empty, $"The value for this field can't be null or whitespace.");

            if (x.Value is null) 
                return new(string.Empty, $"The value for this field can't be null or whitesapce");

            return default;
        });
    }
    public Actions NotNull()
    {
        return AddValidator((x) =>
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
    public static ValidationError Exists(this IList items, Field field)
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