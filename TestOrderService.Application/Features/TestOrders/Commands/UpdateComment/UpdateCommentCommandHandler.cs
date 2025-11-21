using Mapster;
using TestOrderService.Application.DTOs;
using TestOrderService.Application.Exceptions;
using TestOrderService.Application.Interfaces;
using TestOrderService.Application.Interfaces.gRPC;
using TestOrderService.Application.Interfaces.Message;
using TestOrderService.Domain.Entities;
using UnauthorizedAccessException=TestOrderService.Application.Exceptions.UnauthorizedAccessException;

namespace TestOrderService.Application.Features.TestOrders.Commands.UpdateComment
{
    /// <summary>
    ///     Command handler for updating a comment
    /// </summary>
    /// <seealso cref="ICommandHandler{UpdateCommentCommand,CommentResponseDto}" />
    public class UpdateCommentCommandHandler(
        ICommentRepository commentRepository,
        IUnitOfWork unitOfWork,
        IUserGrpcClient userGrpcClient) : ICommandHandler<UpdateCommentCommand, CommentDto>
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
        /// <exception cref="NotFoundException">Thrown when comment not found</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when user not found in IAM service</exception>
        /// <exception cref="ForbiddenException">Thrown when user lacks permission</exception>
        public async Task<CommentDto> Handle(
            UpdateCommentCommand request,
            CancellationToken cancellationToken)
        {

            // Find comment by ID and test order ID
            var comment = await _commentRepository.GetCommentByIdAndTestOrderIdAsync(
                request.CommentId,
                request.TestOrderId,
                cancellationToken);

            if (comment == null)
            {
                throw new NotFoundException(nameof(Comment), request.CommentId);
            }

            // Check if not owner
            if (request.UserId != comment.CreateById)
            {
                throw new UnauthorizedAccessException("User cannot update other user's comments");
            }

            // Update comment properties
            comment.Content = request.Content;
            comment.UpdateById = request.UserId;
            comment.UpdateByName = request.Name;
            comment.UpdateAt = DateTime.UtcNow;

            // Save changes to database
            await _commentRepository.UpdateCommentAsync(comment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Map to response and add user names
            var response = comment.Adapt<CommentDto>();

            return response;
        }
    }
}
