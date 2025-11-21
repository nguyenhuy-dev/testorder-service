using TestOrderService.Application.DTOs;
using TestOrderService.Application.Interfaces.Message;
namespace TestOrderService.Application.Features.TestOrders.Commands.UpdateComment
{
    /// <summary>
    ///     Command for updating a comment
    /// </summary>
    public class UpdateCommentCommand : ICommand<CommentDto>
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
