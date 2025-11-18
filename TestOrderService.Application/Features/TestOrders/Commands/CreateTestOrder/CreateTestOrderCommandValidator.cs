using FluentValidation;
namespace TestOrderService.Application.Features.TestOrders.Commands.CreateTestOrder
{
    /// <summary>
    ///     Validator supports create test order command.
    /// </summary>
    /// <seealso
    ///     cref="AbstractValidator&lt;CreateTestOrderCommand&gt;" />
    public class CreateTestOrderCommandValidator : AbstractValidator<CreateTestOrderCommand>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CreateTestOrderCommandValidator" /> class.
        /// </summary>
        public CreateTestOrderCommandValidator()
        {
            RuleFor(x => x.PatientId)
                .NotEmpty();

            RuleFor(x => x.RunById)
                .NotEmpty();

            RuleFor(x => x.RunAt)
                .GreaterThan(DateTime.UtcNow)
                .NotEmpty();
        }
    }
}
