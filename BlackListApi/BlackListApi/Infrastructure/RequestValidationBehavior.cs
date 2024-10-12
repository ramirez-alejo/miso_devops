using FluentValidation;
using Mediator;

namespace BlackListApi.Infrastructure;
public class RequestValidationBehavior<TRequest, TResponse>
	: AbstractValidator<TRequest>,
		IPipelineBehavior<TRequest, TResponse>
	where TRequest : IRequest<TResponse>
{
	private readonly IEnumerable<IValidator<TRequest>> _validators;

	public RequestValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
	{
		_validators = validators;
	}

	public RequestValidationBehavior() : base()
	{
	}
	
	public async ValueTask<TResponse> Handle(TRequest request, CancellationToken cancellationToken, MessageHandlerDelegate<TRequest,TResponse> next)
	{
		if (!_validators.Any()) return await next(request, cancellationToken);
		var context = new ValidationContext<TRequest>(request);

		var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
		var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

		if (failures.Count != 0)
		{
			throw new ValidationException(failures);
		}
		return await next(request, cancellationToken);
	}
}