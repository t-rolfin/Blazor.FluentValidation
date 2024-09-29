namespace Rolfin.Blazor.FluentValidation;

public partial class FluentFormValidator<Model> : ComponentBase, IDisposable
{
    ValidationMessageStore _store;
    ValidationBuilder<Model> _builder = new();


    [CascadingParameter] 
    public EditContext Context { get; set; }

    [Parameter] public Action<ValidationBuilder<Model>> Roles { get; set; }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (Context is null)
            throw new NullReferenceException($"{nameof(FluentFormValidator<Model>)} must be placed within a {nameof(EditForm)}");

        _store ??= new ValidationMessageStore(Context);

        if(Roles is not null)
            Roles.Invoke(_builder);

        Context.OnValidationRequested -= ValidationRequestedMethod;
        Context.OnValidationRequested += ValidationRequestedMethod;

        Context.OnFieldChanged -= FieldChengedMethod;
        Context.OnFieldChanged += FieldChengedMethod;
    }

    private void FieldChengedMethod(object sender, FieldChangedEventArgs e)
        => _builder.ValidateField(_store, (EditContext)sender, e.FieldIdentifier);

    private void ValidationRequestedMethod(object sender, ValidationRequestedEventArgs e)
        => _builder.Validate(_store, (EditContext)sender);

    public void ChangeRoles(Action<ValidationBuilder<Model>> roles)
    {
        _builder = new();
        roles.Invoke(_builder);
    }
    public void AddErroFor(string fieldName, string ErrorMessage)
    {
        var fieldIdentitifier = Context.Field(fieldName);
        _store.Add(fieldIdentitifier, ErrorMessage);
    }

    public void Dispose()
    {
        Context.OnValidationRequested -= ValidationRequestedMethod;
        Context.OnFieldChanged -= FieldChengedMethod;
    }
}