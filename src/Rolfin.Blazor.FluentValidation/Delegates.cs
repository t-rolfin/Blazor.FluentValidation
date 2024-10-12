namespace Rolfin.Blazor.FluentValidation;

public delegate ValidationError Filter(Field fieldProperties);
public delegate void RulesBuilder<T>(ValidationBuilder<T> builder);
