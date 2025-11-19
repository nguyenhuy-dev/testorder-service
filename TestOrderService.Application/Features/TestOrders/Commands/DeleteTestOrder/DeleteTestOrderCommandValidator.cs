using FluentValidation;
namespace TestOrderService.Application.Features.TestOrders.Commands.DeleteTestOrder
{
    /// <summary>
    ///     Validator cho DeleteTestOrderCommand.
    /// </summary>
    public class DeleteTestOrderCommandValidator : AbstractValidator<DeleteTestOrderCommand>
    {
        public DeleteTestOrderCommandValidator()
        {
            RuleFor(x => x.TestOrderId)
                .NotEmpty()
                .WithMessage("TestOrderId is required.");
        }
    }
}
