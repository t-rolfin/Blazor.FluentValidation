namespace Rolfin.Blazor.FluentValidation;

public record FluentValidationError(string FieldName, string Message);
public record FieldProperties(string Name, object Value);