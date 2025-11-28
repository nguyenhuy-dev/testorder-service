using TestOrderService.Application.DTOs;
using TestOrderService.Application.Interfaces.Message;
namespace TestOrderService.Application.Features.MedicalTestResults.Commands.AddTestResultComment
{
    public class AddTestResultCommentCommand : ICommand<TestResultCommentDto>
    {
        public Guid TestResultId { get; set; }
        public string Content { get; set; } = default!;
        public Guid UserId { get; set; } // From authentication token
        public string Name { get; set; } = default!;
    }
}
