using FluentValidation;
namespace TestOrderService.Application.Features.TestOrder.Commands.CreateTestOrder
{
    /// <summary>
    ///     Validator supports create test order command.
    /// </summary>
    /// <seealso
    ///     cref="FluentValidation.AbstractValidator&lt;TestOrderService.Application.Features.TestOrder.Commands.CreateTestOrder.CreateTestOrderCommand&gt;" />
    public class CreateTestOrderCommandValidator : AbstractValidator<CreateTestOrderCommand>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CreateTestOrderCommandValidator" /> class.
        /// </summary>
        public CreateTestOrderCommandValidator()
        {
            RuleFor(x => x.RunById)
                .NotEmpty();
        }
    }
}
