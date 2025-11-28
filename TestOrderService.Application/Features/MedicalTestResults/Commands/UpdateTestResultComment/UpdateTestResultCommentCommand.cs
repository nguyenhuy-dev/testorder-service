using TestOrderService.Application.DTOs;
using TestOrderService.Application.Interfaces.Message;
namespace TestOrderService.Application.Features.MedicalTestResults.Commands.UpdateTestResultComment
{
    /// <summary>
    ///     Command for updating a test result comment
    /// </summary>
    public class UpdateTestResultCommentCommand : ICommand<TestResultCommentDto>
    {
        /// <summary>
        ///     Gets or sets the test result identifier.
        /// </summary>
        public Guid TestResultId { get; set; }

        /// <summary>
        ///     Gets or sets the comment identifier.
        /// </summary>
        public Guid CommentId { get; set; }

        /// <summary>
        ///     Gets or sets the updated content.
        /// </summary>
        public string Content { get; set; } = default!;

        /// <summary>
        ///     Gets or sets the user identifier (from JWT token).
        /// </summary>
        public Guid UserId { get; set; }

        public string Name { get; set; } = default!;
    }
}
