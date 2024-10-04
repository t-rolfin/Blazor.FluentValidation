namespace Rolfin.Blazor.FluentValidation;

public record ValidationError(string FieldName, string Message);
public record FieldProperties(string Name, object Value);