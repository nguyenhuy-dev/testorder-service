using Mapster;
using TestOrderService.Application.DTOs;
using TestOrderService.Application.Exceptions;
using TestOrderService.Application.Interfaces;
using TestOrderService.Application.Interfaces.gRPC;
using TestOrderService.Application.Interfaces.Message;
using TestOrderService.Domain.Entities;
namespace TestOrderService.Application.Features.TestOrders.Commands.AddComment
{
    /// <summary>
    ///     Command handler for adding a comment to a test order
    /// </summary>
    /// <seealso cref="ICommandHandler{AddCommentCommand,CommentResponseDto}" />
    public class AddCommentCommandHandler(
        ICommentRepository commentRepository,
        IUnitOfWork unitOfWork,
        IUserGrpcClient userGrpcClient) : ICommandHandler<AddCommentCommand, CommentDto>
    {
        /// <summary>
        ///     The comment repository
        /// </summary>
        private readonly ICommentRepository _commentRepository = commentRepository;

        /// <summary>
        ///     The unit of work
        /// </summary>
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        /// <summary>
        ///     The user gRPC client
        /// </summary>
        private readonly IUserGrpcClient _userGrpcClient = userGrpcClient;

        /// <summary>
        ///     Handles the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.NotFoundException">Thrown when test order not found</exception>
        /// <exception cref="System.UnauthorizedAccessException">Thrown when user not found in IAM service</exception>
        /// <exception cref="ForbiddenException">Thrown when user lacks permission</exception>
        public async Task<CommentDto> Handle(
            AddCommentCommand request,
            CancellationToken cancellationToken)
        {
            // Verify test order exists
            var testOrderExists = await _commentRepository.TestOrderExistsAsync(
                request.TestOrderId,
                cancellationToken);

            if (!testOrderExists)
            {
                throw new NotFoundException(nameof(TestOrder), request.TestOrderId);
            }

            // Check if user has authorized role (Not implemented)

            var comment = new Comment
            {
                CommentId = Guid.NewGuid(),
                Content = request.Content,
                CreateById = request.UserId,
                CreateByName = request.Name,
                CreateAt = DateTime.UtcNow,
                TestOrderId = request.TestOrderId
            };

            // Save to database
            var createdComment = await _commentRepository.CreateCommentAsync(comment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Map to response and add user name
            var response = createdComment.Adapt<CommentDto>();

            return response;
        }
    }
}
