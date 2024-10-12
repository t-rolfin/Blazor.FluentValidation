namespace Rolfin.Blazor.FluentValidation;


public record ValidationError(string FieldName, string Message);
public record Field(string Name, object Value);


abstract record Validator<T>(string Name, List<Filter> Filters, Delegate If, Delegate ValueHandler);

record PropertyValidator(string Name, List<Filter> Filters, Delegate If, Delegate ValueHandler)
	: Validator<object>(Name, Filters, If, ValueHandler);

record ListValidator(string Name, List<Filter> Filters, Delegate If, Delegate ValueHandler, Expression<Func<object, List<object>>> ListIdentifier)
    : Validator<object>(Name, Filters, If, ValueHandler);
