using TestOrderService.Application.Interfaces.Message;
namespace TestOrderService.Application.Features.MedicalTestResults.Commands.DeleteTestResultComment
{
    /// <summary>
    ///     Command for deleting a test result comment
    /// </summary>
    public class DeleteTestResultCommentCommand : ICommand<bool>
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
        ///     Gets or sets the user identifier (from JWT token).
        /// </summary>
        public Guid UserId { get; set; }

        public string Role { get; set; } = default!;
    }
}
