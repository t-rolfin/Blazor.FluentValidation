using Microsoft.AspNetCore.Components.Rendering;


namespace Rolfin.Blazor.FluentValidation;


public class FluentValidationMessage<TValue> : ComponentBase, IDisposable
{

    private readonly EventHandler<ValidationStateChangedEventArgs> _validationStateChangedHandler;
    private Expression<Func<TValue>> _previousFieldAccessor;
    private Func<string> _previousRowIdentifier;
    private EditContext _previousEditContext;
    private FieldIdentifier _fieldIdentifier;


    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

    [CascadingParameter]
    public EditContext CurrentEditContext { get; set; }


    [Parameter]
    public Expression<Func<TValue>> For { get; set; }

    [Parameter]
    public Func<string> RowIdentifier { get; set; }


    public FluentValidationMessage()
    {
        _validationStateChangedHandler += delegate
        {
            StateHasChanged();
        };
    }

    protected override void OnParametersSet()
    {
        _ = CurrentEditContext ?? throw new InvalidOperationException();
        _ = For ?? throw new InvalidOperationException();

        if(For != _previousFieldAccessor)
        {
            var field = FieldIdentifier.Create(For);

            _fieldIdentifier = RowIdentifier == _previousRowIdentifier
                ? field
                : new(CurrentEditContext.Model, $"{RowIdentifier.Invoke()}.{field.FieldName}");

            _previousRowIdentifier = RowIdentifier;
            _previousFieldAccessor = For;
        }

        if(CurrentEditContext != _previousEditContext)
        {
            DetachValidationStateChangedListener();
            CurrentEditContext.OnValidationStateChanged += _validationStateChangedHandler;
            _previousEditContext = CurrentEditContext;
        }
    }
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        foreach(var validationMessage in CurrentEditContext.GetValidationMessages(_fieldIdentifier))
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "validation-message");
            builder.AddMultipleAttributes(2, AdditionalAttributes);
            builder.AddContent(3, validationMessage);
            builder.CloseElement();
        }
    }


    protected virtual void Dispose(bool disposing)
    {
    }


    void IDisposable.Dispose()
    {
        DetachValidationStateChangedListener();
        Dispose(disposing: true);
    }
    void DetachValidationStateChangedListener()
    {
        if (_previousEditContext != null)
        {
            _previousEditContext.OnValidationStateChanged -= _validationStateChangedHandler;
        }
    }
}


