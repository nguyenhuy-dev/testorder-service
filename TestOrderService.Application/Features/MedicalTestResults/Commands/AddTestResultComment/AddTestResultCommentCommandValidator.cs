using FluentValidation;
namespace TestOrderService.Application.Features.MedicalTestResults.Commands.AddTestResultComment
{
    public class AddTestResultCommentCommandValidator : AbstractValidator<AddTestResultCommentCommand>
    {
        public AddTestResultCommentCommandValidator()
        {
            RuleFor(x => x.TestResultId)
                .NotEmpty()
                .WithMessage("Test result ID is required.");

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
