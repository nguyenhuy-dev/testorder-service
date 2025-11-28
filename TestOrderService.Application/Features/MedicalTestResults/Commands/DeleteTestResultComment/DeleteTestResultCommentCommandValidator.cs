using FluentValidation;
namespace TestOrderService.Application.Features.MedicalTestResults.Commands.DeleteTestResultComment
{
    /// <summary>
    ///     Validator for DeleteTestResultCommentCommand
    /// </summary>
    public class DeleteTestResultCommentCommandValidator : AbstractValidator<DeleteTestResultCommentCommand>
    {
        public DeleteTestResultCommentCommandValidator()
        {
            RuleFor(x => x.TestResultId)
                .NotEmpty()
                .WithMessage("Test result ID is required.");

            RuleFor(x => x.CommentId)
                .NotEmpty()
                .WithMessage("Comment ID is required.");

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required.");
        }
    }
}
