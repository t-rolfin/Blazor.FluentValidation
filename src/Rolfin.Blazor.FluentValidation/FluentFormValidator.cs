namespace Rolfin.Blazor.FluentValidation;

public class FluentFormValidator<TModel> : ComponentBase, IDisposable
{
    ValidationMessageStore _store;
    ValidationBuilder<TModel> _builder;
    RulesBuilder<TModel> _rules;
    private EditContext _previousEditContext;

    [CascadingParameter]
    public EditContext CurrentContext { get; set; }

    [Parameter] public RulesBuilder<TModel> Rules { get => _rules; set => _rules = value; }


    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (CurrentContext is null)
            throw new NullReferenceException($"{nameof(FluentFormValidator<TModel>)} must be placed within a {nameof(EditForm)}");

        _store ??= new ValidationMessageStore(CurrentContext);

        if (Rules is not null && _builder is null) _builder ??= ValidationBuilder<TModel>.Create(Rules);

        if(CurrentContext != _previousEditContext)
        {
            DetachEditContextEvents();
            CurrentContext.OnValidationRequested += ValidationRequestedMethod;
            CurrentContext.OnFieldChanged += FieldChengedMethod;
            _previousEditContext = CurrentContext;
        }
    }


    void FieldChengedMethod(object sender, FieldChangedEventArgs e) => _builder.ValidateField(_store, (EditContext)sender, e);
    void ValidationRequestedMethod(object sender, ValidationRequestedEventArgs e) => _builder.Validate(_store, (EditContext)sender);


	public FluentFormValidator<TModel> ChangeRules(RulesBuilder<TModel> rules)
    {
        _store.Clear();
        _rules = rules;
        _builder = ValidationBuilder<TModel>.Create(rules);

        return this;
    }
    public Actions AddRule(Expression<Func<TModel, object>> property, bool removeIfRule = false) => _builder.AddRule(property, removeIfRule);
    public void AddErroFor(string fieldName, string ErrorMessage)
    {
        var fieldIdentitifier = CurrentContext.Field(fieldName);
        _store.Add(fieldIdentitifier, ErrorMessage);
    }
    public void ClearErrorsFor(string fieldName)
    {
        var fieldIdentitifier = CurrentContext.Field(fieldName);
        _store.Clear(fieldIdentitifier);
    }


    public bool Validate()
    {
        _builder.Validate(_store, CurrentContext);
        return CurrentContext.GetValidationMessages().Any() is false;
    }


    protected virtual void Dispose(bool disposing)
    {
    }

    void IDisposable.Dispose()
    {
        DetachEditContextEvents();
        Dispose(disposing: true);
    }
    void DetachEditContextEvents()
    {
        CurrentContext.OnValidationRequested -= ValidationRequestedMethod;
        CurrentContext.OnFieldChanged -= FieldChengedMethod;
    }
}