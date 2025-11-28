using FluentValidation;
namespace TestOrderService.Application.Features.MedicalTestResults.Commands.UpdateTestResultComment
{
    /// <summary>
    ///     Validator for UpdateTestResultCommentCommand
    /// </summary>
    public class UpdateTestResultCommentCommandValidator : AbstractValidator<UpdateTestResultCommentCommand>
    {
        public UpdateTestResultCommentCommandValidator()
        {
            RuleFor(x => x.TestResultId)
                .NotEmpty()
                .WithMessage("Test result ID is required.");

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
