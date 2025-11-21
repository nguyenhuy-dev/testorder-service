using TestOrderService.Application.Interfaces.Message;
namespace TestOrderService.Application.Features.TestOrders.Commands.DeleteComment
{
    /// <summary>
    ///     Command for deleting a comment
    /// </summary>
    public class DeleteCommentCommand : ICommand<bool>
    {
        /// <summary>
        ///     Gets or sets the test order identifier.
        /// </summary>
        public Guid TestOrderId { get; set; }

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
