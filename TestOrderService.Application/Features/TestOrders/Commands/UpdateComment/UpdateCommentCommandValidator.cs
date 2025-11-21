using FluentValidation;
namespace TestOrderService.Application.Features.TestOrders.Commands.UpdateComment
{
    /// <summary>
    ///     Validator for UpdateCommentCommand
    /// </summary>
    public class UpdateCommentCommandValidator : AbstractValidator<UpdateCommentCommand>
    {
        public UpdateCommentCommandValidator()
        {
            RuleFor(x => x.TestOrderId)
                .NotEmpty()
                .WithMessage("Test order ID is required.");

            RuleFor(x => x.CommentId)
                .NotEmpty()
                .WithMessage("Comment ID is required.");

            RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage("Content is required.")
                .MaximumLength(2000)
                .WithMessage("Content must not exceed 2000 characters.");

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required.");
        }
    }
}
