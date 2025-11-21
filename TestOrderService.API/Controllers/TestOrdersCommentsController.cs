using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestOrderService.API.Commons;
using TestOrderService.API.Middleware;
using TestOrderService.Application.DTOs;
using TestOrderService.Application.Features.TestOrders.Commands.AddComment;
using TestOrderService.Application.Features.TestOrders.Commands.DeleteComment;
using TestOrderService.Application.Features.TestOrders.Commands.UpdateComment;
namespace TestOrderService.API.Controllers
{
    /// <summary>
    ///     Test order comments APIs.
    /// </summary>
    [Route("api/TestOrders/{testOrderId:guid}/comments")]
    public class TestOrderCommentsController(ISender sender) : BaseApiController
    {
        private readonly ISender _sender = sender;

        /// <summary>
        ///     Add a comment to a test order
        /// </summary>
        [HttpPost]
        [Authorize("add_comment")]
        [ProducesResponseType(typeof(ApiResponse<CommentDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<CommentDto>>> AddComment(
            [FromRoute] Guid testOrderId,
            [FromBody] CommentRequestDto request)
        {
            var command = new AddCommentCommand
            {
                TestOrderId = testOrderId,
                Content = request.Content,
                UserId = CurrentUserId, // Using BaseController property
                Name = CurrentUserName // Using BaseController property
            };

            var result = await _sender.Send(command);

            return CreatedAtAction(
                nameof(AddComment),
                new
                {
                    testOrderId, commentId = result.CommentId
                },
                ApiResponse<CommentDto>.Success(result, "Comment added successfully."));
        }

        /// <summary>
        ///     Update a comment on a test order
        /// </summary>
        [HttpPut("{commentId:guid}")]
        [Authorize("modify_comment")]
        [ProducesResponseType(typeof(ApiResponse<CommentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<CommentDto>>> UpdateComment(
            [FromRoute] Guid testOrderId,
            [FromRoute] Guid commentId,
            [FromBody] CommentRequestDto request)
        {
            var command = new UpdateCommentCommand
            {
                TestOrderId = testOrderId,
                CommentId = commentId,
                Content = request.Content,
                UserId = CurrentUserId,
                Name = CurrentUserName
            };

            var result = await _sender.Send(command);

            return Ok(ApiResponse<CommentDto>.Success(result, "Comment modified successfully."));
        }

        /// <summary>
        ///     Delete a comment from a test order
        /// </summary>
        [HttpDelete("{commentId:guid}")]
        [Authorize("delete_comment")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteComment(
            [FromRoute] Guid testOrderId,
            [FromRoute] Guid commentId)
        {
            var command = new DeleteCommentCommand
            {
                TestOrderId = testOrderId,
                CommentId = commentId,
                UserId = CurrentUserId,
                Role = CurrentUserRole
            };

            var result = await _sender.Send(command);

            return Ok(ApiResponse<bool>.Success(result, "Comment deleted successfully."));
        }
    }
}
