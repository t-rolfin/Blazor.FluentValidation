namespace Rolfin.Blazor.FluentValidation;


public class FluentFormValidator<TModel> : ComponentBase, IDisposable
{
    bool _suppressValidationRequested = false;


    RulesBuilder<TModel> _rules = (builder) => { };
    ValidationBuilder<TModel> _builder;
    EditContext _previousEditContext;
    ValidationMessageStore _store;


    [CascadingParameter]  EditContext CurrentContext { get; set; }
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


    void FieldChengedMethod(object sender, FieldChangedEventArgs e) 
    {
        if (_builder is null) throw new ArgumentNullException("Validation rules aren't set.");
        _builder.ValidateField(_store, (EditContext)sender, e);
    } 
    void ValidationRequestedMethod(object sender, ValidationRequestedEventArgs e)
    {
        if (_builder is null) throw new ArgumentNullException("Validation rules aren't set.");
        if (_suppressValidationRequested)
        {
			_store.Clear();
            return;
		}
		_builder.Validate(_store, (EditContext)sender);
	}


    /// <summary>
    /// Replace current rules.
    /// </summary>
	public FluentFormValidator<TModel> ChangeRules(RulesBuilder<TModel> rules)
    {
        _store.Clear();
        _rules = rules;
        _builder = ValidationBuilder<TModel>.Create(rules);

        return this;
    }

    /// <summary>
    /// Adds new rules to existing ones.
    /// </summary>
    public FluentFormValidator<TModel> AddRules(RulesBuilder<TModel> rules)
    {
        var secBuilder = ValidationBuilder<TModel>.Create(rules);
        _builder.Join(secBuilder);
        return this;
    }
    public Actions AddRule(Expression<Func<TModel, object>> property, bool removeIfRule = false)
    {
        if (_builder is null) throw new ArgumentNullException("Validation rules aren't set.");

        _suppressValidationRequested = true;
        return _builder.AddRule(property, removeIfRule);
	}
    public void AddErrorFor(string fieldName, string ErrorMessage)
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