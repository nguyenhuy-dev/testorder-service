using FluentValidation;
using MediatR;
using ValidationException=TestOrderService.Application.Exceptions.ValidationException;
namespace TestOrderService.Application.Behaviors
{
    /// <summary>
    ///     Validation behavior supports for building validation exception.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <seealso cref="MediatR.IPipelineBehavior&lt;TRequest, TResponse&gt;" />
    public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validatiors) : IPipelineBehavior<TRequest, TResponse> where TRequest : class, IRequest<TResponse>
    {
        /// <summary>
        ///     The validators
        /// </summary>
        private readonly IEnumerable<IValidator<TRequest>> _validators = validatiors;

        /// <summary>
        ///     Pipeline handler. Perform any additional behavior and await the <paramref name="next" /> delegate as necessary
        /// </summary>
        /// <param name="request">Incoming request</param>
        /// <param name="next">
        ///     Awaitable delegate for the next action in the pipeline. Eventually this delegate represents the
        ///     handler.
        /// </param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>
        ///     Awaitable task returning the <typeparamref name="TResponse" />
        /// </returns>
        /// <exception cref="TestOrderService.Application.Exceptions.ValidationException"></exception>
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!_validators.Any())
                return await next(cancellationToken);

            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken))
            );

            var errorsDictionary = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .GroupBy(
                    x => x.PropertyName,
                    x => x.ErrorMessage,
                    (propertyName, errorMessage) => new
                    {
                        Key = propertyName,
                        Values = errorMessage.Distinct().ToArray()
                    }
                ).ToDictionary(x => x.Key, x => x.Values);

            if (errorsDictionary.Count != 0)
                throw new ValidationException(errorsDictionary);

            return await next(cancellationToken);
        }
    }
}
