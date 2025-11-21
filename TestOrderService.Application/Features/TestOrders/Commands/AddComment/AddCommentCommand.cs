using TestOrderService.Application.DTOs;
using TestOrderService.Application.Interfaces.Message;
namespace TestOrderService.Application.Features.TestOrders.Commands.AddComment
{
    public class AddCommentCommand : ICommand<CommentDto>
    {
        public Guid TestOrderId { get; set; }
        public string Content { get; set; } = default!;
        public Guid UserId { get; set; } // From authentication token
        public string Name { get; set; } = default!;
    }
}
