using MediatR;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Behaviors
{
	/// <summary>
	/// Pipeline behavior that validates requests before they reach handlers
	/// Runs automatically for all commands/queries
	/// </summary>
	public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
		where TRequest : IRequest<TResponse>
	{
		private readonly IEnumerable<IValidator<TRequest>> _validators;
		private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

		public ValidationBehavior(
			IEnumerable<IValidator<TRequest>> validators,
			ILogger<ValidationBehavior<TRequest, TResponse>> logger)
		{
			_validators = validators;
			_logger = logger;
		}

		public async Task<TResponse> Handle(
			TRequest request,
			RequestHandlerDelegate<TResponse> next,
			CancellationToken cancellationToken)
		{
			// If no validators, skip validation
			if (!_validators.Any())
			{
				return await next();
			}

			var context = new ValidationContext<TRequest>(request);

			_logger.LogInformation("Validating {RequestType}", typeof(TRequest).Name);

			// Run all validators
			var validationResults = await Task.WhenAll(
				_validators.Select(v => v.ValidateAsync(context, cancellationToken)));

			// Collect failures
			var failures = validationResults
				.SelectMany(r => r.Errors)
				.Where(f => f != null)
				.ToList();

			// If validation failed, throw exception
			if (failures.Any())
			{
				_logger.LogWarning("Validation failed for {RequestType}: {Errors}",
					typeof(TRequest).Name,
					string.Join(", ", failures.Select(f => f.ErrorMessage)));

				throw new ValidationException(failures);
			}

			_logger.LogInformation("Validation successful for {RequestType}", typeof(TRequest).Name);

			// Proceed to handler
			return await next();
		}
	}
}