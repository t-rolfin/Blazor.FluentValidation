namespace Rolfin.Blazor.FluentValidation;

class PropertyValidation<T>
{
    public string Name { get; set; }
    internal Expression<Func<T, bool>> _if; 
    internal Expression<Func<T, object>> _property;
    internal List<Func<FieldProperties, FluentValidationError>> _filters = new();

    public PropertyValidation(Expression<Func<T, object>> property, string name)
    {
         _property = property;
        Name = name;
    }
}

