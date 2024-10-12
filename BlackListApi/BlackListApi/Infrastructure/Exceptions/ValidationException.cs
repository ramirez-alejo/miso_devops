using System.Diagnostics.CodeAnalysis;
using FluentValidation.Results;

namespace BlackListApi.Infrastructure.Exceptions;

[ExcludeFromCodeCoverage]
public class ValidationException : Exception
{
	private readonly string _message = "One or more validation failures have occurred.";
	public override string Message => _message ?? base.Message;
	public ValidationException(string message) : base(message)
	{
		Failures = new Dictionary<string, string[]> { { "error", new[] { message } } };
	}
	
	public ValidationException() : base("One or more validation failures have occurred.")
	{
		Failures = new Dictionary<string, string[]>();
	}

	public ValidationException(IEnumerable<ValidationFailure> failures) : this()
	{
		var failureGroups = failures.GroupBy(e => e.PropertyName, e => e.ErrorMessage);

		foreach (var failureGroup in failureGroups)
		{
			var propertyName = failureGroup.Key;
			var propertyFailures = failureGroup.ToArray();

			Failures.Add(propertyName, propertyFailures);
		}

		if (Failures.Any())
		{
			var firstFailure = Failures.First();
			_message = $"One or more validation failures have occurred. {firstFailure.Value.First()}";
		}
	}
	
	/// <summary>
	/// ValidationException constructor which takes a property and an error message
	/// </summary>
	public ValidationException(string property, string errorMessage) : this(new List<ValidationFailure>() { new ValidationFailure(property, errorMessage) })
	{
	}

	public IDictionary<string, string[]> Failures { get; } = new Dictionary<string, string[]>();
}