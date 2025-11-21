using FluentValidation;
namespace TestOrderService.Application.Features.TestOrders.Commands.DeleteComment
{
    /// <summary>
    ///     Validator for DeleteCommentCommand
    /// </summary>
    public class DeleteCommentCommandValidator : AbstractValidator<DeleteCommentCommand>
    {
        public DeleteCommentCommandValidator()
        {
            RuleFor(x => x.TestOrderId)
                .NotEmpty()
                .WithMessage("Test order ID is required.");

            RuleFor(x => x.CommentId)
                .NotEmpty()
                .WithMessage("Comment ID is required.");

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required.");
        }
    }
}
