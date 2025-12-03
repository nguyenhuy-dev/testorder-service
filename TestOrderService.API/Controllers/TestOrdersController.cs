using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TestOrderService.API.Commons;
using TestOrderService.API.Middleware;
using TestOrderService.Application.DTOs;
using TestOrderService.Application.Features.TestOrders.Commands.CreateTestOrder;
using TestOrderService.Application.Features.TestOrders.Commands.DeleteTestOrder;
using TestOrderService.Application.Features.TestOrders.Commands.UpdateTestOrder;
using TestOrderService.Application.Features.TestOrders.Queries.GetDetail;
using TestOrderService.Application.Features.TestOrders.Queries.GetTestOrders;
using TestOrderService.Application.Features.TestOrders.Queries.GetTestOrdersByPatient;
using TestOrderService.Domain.Entities;
namespace TestOrderService.API.Controllers
{
    /// <summary>
    ///     Test orders apis.
    /// </summary>
    [Route("api/[controller]")]
    public class TestOrdersController(ISender sender) : BaseApiController
    {
        private readonly ISender _sender = sender;

        /// <summary>
        ///     Creates the test order.
        /// </summary>
        /// <param name="patientId">The patient identifier.</param>
        /// <param name="createTestOrderDto">The create test order dto.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">Can't parse '{nameof(createById)}' to Guid: {createById}.</exception>
        [HttpPost]
        [Authorize(Policy = "create_test_order")]
        [ProducesResponseType(typeof(ApiResponse<TestOrder>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateTestOrder([FromBody] CreateTestOrderDto createTestOrderDto, CancellationToken cancellationToken = default)
        {
            var testOrderCommand = createTestOrderDto.Adapt<CreateTestOrderCommand>();

            var createById = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(createById, out var createByIdGuid))
                throw new InvalidOperationException($"Can't parse '{nameof(createById)}' to Guid: {createById}.");
            testOrderCommand.CreateById = createByIdGuid;

            testOrderCommand.PatientId = createTestOrderDto.PatientId;
            testOrderCommand.TestOrderDescription = createTestOrderDto.TestOrderDescription;

            var testOrder = await _sender.Send(testOrderCommand, cancellationToken);

            var response = new ApiResponse<TestOrder>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "Create test order successfully",
                Data = testOrder
            };

            return StatusCode(StatusCodes.Status201Created, response);
        }

        /// <summary>
        ///     Updates the test order using the specified id
        /// </summary>
        /// <param name="id">The test order identifier.</param>
        /// <param name="updateTestOrderDto">The update test order dto.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task containing the action result</returns>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<TestOrder>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateTestOrder(Guid id, [FromBody] UpdateTestOrderDto updateTestOrderDto, CancellationToken cancellationToken = default)
        {
            var updateTestOrderCommand = updateTestOrderDto.Adapt<UpdateTestOrderCommand>();
            updateTestOrderCommand.TestOrderId = id;

            // Using BaseApiController helper property
            updateTestOrderCommand.UpdateById = CurrentUserId;

            var testOrder = await _sender.Send(updateTestOrderCommand, cancellationToken);

            return Ok(new ApiResponse<TestOrderDto>
            {
                StatusCode = 200,
                Message = "Test order updated successfully.",
                Data = testOrder
            });
        }

        /// <summary>
        ///     Deletes the test order using the specified id
        /// </summary>
        /// <param name="id">The id</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task containing the action result</returns>
        [HttpDelete("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
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

            return Ok(new ApiResponse<object>
            {
                StatusCode = 200,
                Message = "Delete TestOrder successfully.",
                Data = new { TestOrderId = id, Deleted = result }
            });
        }

        /// <summary>
        ///     Gets the test orders.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PaginatedList<TestOrderDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
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
        [HttpGet("{id:guid}/detail")]
        [ProducesResponseType(typeof(ApiResponse<TestOrderDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetTestOrderDetail(Guid id, CancellationToken ct)
        {
            var query = new GetTestOrderDetailQuery(id);
            var result = await _sender.Send(query, ct);

            return Ok(ApiResponse<TestOrderDetailDto>.Success(result));
        }

        /// <summary>
        ///     Gets all test orders for a specific patient by userId or patientId
        /// </summary>
        /// <param name="id">The user identifier or patient identifier</param>
        /// <param name="ct">The cancellation token</param>
        /// <returns>List of test orders for the patient</returns>
        [HttpGet("patient/{id:guid}")]
        [Authorize] // Any authenticated user can call this
        [ProducesResponseType(typeof(ApiResponse<List<TestOrderDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTestOrdersByPatient(Guid id, CancellationToken ct)
        {
            // Security: Patient can only view their own test orders
            // Admin and Lab Users can view any patient's test orders
            var currentUserId = CurrentUserId;
            var isAdmin = User.IsInRole("Admin");
            var isLabUser = User.IsInRole("Lab User");

            if (!isAdmin && !isLabUser && currentUserId != id)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse
                {
                    StatusCode = 403,
                    Message = "You do not have permission to view these test orders."
                });
            }

            // Try to get test orders - the repository will handle userId -> patientId mapping
            var query = new GetTestOrdersByPatientQuery(id);
            var result = await _sender.Send(query, ct);

            return Ok(new ApiResponse<List<TestOrderDto>>
            {
                StatusCode = 200,
                Message = "Get test orders successfully.",
                Data = result
            });
        }
    }
}
