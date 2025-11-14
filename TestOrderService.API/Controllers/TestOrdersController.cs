using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TestOrderService.API.Commons;
using TestOrderService.API.Middleware;
using TestOrderService.Application.DTOs;
using TestOrderService.Application.Features.TestOrders.Commands.CreateTestOrder;
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
    }
}
