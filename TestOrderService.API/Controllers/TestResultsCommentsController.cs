using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestOrderService.API.Commons;
using TestOrderService.API.Middleware;
using TestOrderService.Application.DTOs;
using TestOrderService.Application.Features.MedicalTestResults.Commands.AddTestResultComment;
using TestOrderService.Application.Features.MedicalTestResults.Commands.DeleteTestResultComment;
using TestOrderService.Application.Features.MedicalTestResults.Commands.UpdateTestResultComment;
namespace TestOrderService.API.Controllers
{
    /// <summary>
    ///     Test result comments APIs.
    /// </summary>
    [Route("api/TestResults/{testResultId:guid}/comments")]
    public class TestResultCommentsController(ISender sender) : BaseApiController
    {
        private readonly ISender _sender = sender;

        /// <summary>
        ///     Add a comment to a test result
        /// </summary>
        [HttpPost]
        [Authorize("add_comment")]
        [ProducesResponseType(typeof(ApiResponse<TestResultCommentDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<TestResultCommentDto>>> AddTestResultComment(
            [FromRoute] Guid testResultId,
            [FromBody] TestResultCommentRequestDto request)
        {
            var command = new AddTestResultCommentCommand
            {
                TestResultId = testResultId,
                Content = request.Content,
                UserId = CurrentUserId, // Using BaseController property
                Name = CurrentUserName // Using BaseController property
            };

            var result = await _sender.Send(command);

            return CreatedAtAction(
                nameof(AddTestResultComment),
                new
                {
                    testResultId, commentId = result.TestResultCommentId
                },
                ApiResponse<TestResultCommentDto>.Success(result, "Comment added successfully."));
        }

        /// <summary>
        ///     Update a comment on a test result
        /// </summary>
        [HttpPut("{commentId:guid}")]
        [Authorize("modify_comment")]
        [ProducesResponseType(typeof(ApiResponse<TestResultCommentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<TestResultCommentDto>>> UpdateTestResultComment(
            [FromRoute] Guid testResultId,
            [FromRoute] Guid commentId,
            [FromBody] TestResultCommentRequestDto request)
        {
            var command = new UpdateTestResultCommentCommand
            {
                TestResultId = testResultId,
                CommentId = commentId,
                Content = request.Content,
                UserId = CurrentUserId,
                Name = CurrentUserName
            };

            var result = await _sender.Send(command);

            return Ok(ApiResponse<TestResultCommentDto>.Success(result, "Comment modified successfully."));
        }

        /// <summary>
        ///     Delete a comment from a test result
        /// </summary>
        [HttpDelete("{commentId:guid}")]
        [Authorize("delete_comment")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteTestResultComment(
            [FromRoute] Guid testResultId,
            [FromRoute] Guid commentId)
        {
            var command = new DeleteTestResultCommentCommand
            {
                TestResultId = testResultId,
                CommentId = commentId,
                UserId = CurrentUserId,
                Role = CurrentUserRole
            };

            var result = await _sender.Send(command);

            return Ok(ApiResponse<bool>.Success(result, "Comment deleted successfully."));
        }
    }
}
