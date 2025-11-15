using FluentValidation;
using TestOrderService.Application.Features.TestOrders.Commands;
namespace TestOrderService.Application.Validators.TestOrder
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
