using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TestOrderService.API.Commons;
using TestOrderService.API.Middleware;
using TestOrderService.Application.DTOs;
using TestOrderService.Application.Features.TestOrders.Commands;
using TestOrderService.Application.Features.TestOrders.Commands.CreateTestOrder;
using TestOrderService.Application.Features.TestOrders.Queries.GetDetail;
using TestOrderService.Application.Features.TestOrders.Queries.GetTestOrders;
using TestOrderService.Domain.Entities;
namespace TestOrderService.API.Controllers
{
    /// <summary>
    ///     Test orders apis.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase" />
    [Route("api/[controller]"), ApiController]
    public class TestOrdersController(ISender sender) : ControllerBase
    {
        /// <summary>
        ///     The sender
        /// </summary>
        private readonly ISender _sender = sender;

        /// <summary>
        ///     Creates the test order.
        /// </summary>
        /// <param name="patientId">The patient identifier.</param>
        /// <param name="createTestOrderDto">The create test order dto.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">Can't parse '{nameof(createById)}' to Guid: {createById}.</exception>
        [HttpPost("{patientId}"), ProducesResponseType(typeof(ApiResponse<TestOrder>), StatusCodes.Status201Created), ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest), ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized), ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateTestOrder(Guid patientId, [FromBody] CreateTestOrderDto createTestOrderDto, CancellationToken cancellationToken = default)
        {
            var testOrderCommand = createTestOrderDto.Adapt<CreateTestOrderCommand>();

            var createById = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(createById, out var createByIdGuid))
                //throw new InvalidOperationException($"Can't parse '{nameof(createById)}' to Guid: {createById}.");
                testOrderCommand.CreateById = createByIdGuid;

            testOrderCommand.PatientId = patientId;

            var testOrder = await _sender.Send(testOrderCommand, cancellationToken);

            return StatusCode(StatusCodes.Status201Created, testOrder);
        }

        /// <summary>
        ///     Deletes the test order using the specified id
        /// </summary>
        /// <param name="id">The id</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task containing the action result</returns>
        [HttpDelete("{id:guid}"), ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK), ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest), ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound), ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized), ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteTestOrder(Guid id, CancellationToken cancellationToken = default)
        {
            var command = new DeleteTestOrderCommand(id);
            var result = await _sender.Send(command, cancellationToken);

            if (!result)
            {
                return BadRequest(new ErrorResponse
                {
                    StatusCode = 400,
                    Message = "Cannot delete this TestOrder. Only Completed TestOrders can be deleted."
                });
            }

            var response = new ApiResponse<object>
            {
                StatusCode = 200,
                Message = "Delete TestOrder successfully.",
                Data = new { TestOrderId = id, Deleted = result }
            };

            return Ok(response);
        }
        /// <summary>
        ///     Gets the test orders.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpGet, ProducesResponseType(typeof(ApiResponse<PaginatedList<TestOrderDto>>), StatusCodes.Status200OK), ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest), ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTestOrders([FromQuery] GetTestOrdersRequest request, CancellationToken cancellationToken = default)
        {
            var query = new GetTestOrdersQuery(request);
            var result = await _sender.Send(query, cancellationToken);

            return Ok(ApiResponse<PaginatedList<TestOrderDto>>.Success(result));
        }

        /// <summary>
        ///     Gets the test order detail using the specified id
        /// </summary>
        /// <param name="id">The id</param>
        /// <param name="ct">The ct</param>
        /// <returns>A task containing the action result</returns>
        [HttpGet("{id:guid}/detail"), ProducesResponseType(typeof(ApiResponse<TestOrderDetailDto>), StatusCodes.Status200OK), ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound), ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError), ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetTestOrderDetail(Guid id, CancellationToken ct)
        {
            var query = new GetTestOrderDetailQuery(id);
            var result = await _sender.Send(query, ct);

            return Ok(ApiResponse<TestOrderDetailDto>.Success(result));
        }
    }
}
