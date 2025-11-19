using FluentValidation;
namespace TestOrderService.Application.Features.TestOrders.Commands.UpdateTestOrder
{
    /// <summary>
    ///     Validator supports update test order command.
    /// </summary>
    /// <seealso cref="AbstractValidator{UpdateTestOrderCommand}" />
    public class UpdateTestOrderCommandValidator : AbstractValidator<UpdateTestOrderCommand>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="UpdateTestOrderCommandValidator" /> class.
        /// </summary>
        public UpdateTestOrderCommandValidator()
        {
            RuleFor(x => x.TestOrderId)
                .NotEmpty()
                .WithMessage("TestOrderId is required.");

            RuleFor(x => x.UpdateById)
                .NotEmpty()
                .WithMessage("UpdateById is required.");

            // RunById and RunAt are optional and independent during update
            // They can be updated separately or together
            // No cross-validation needed for update operations
        }
    }
}
