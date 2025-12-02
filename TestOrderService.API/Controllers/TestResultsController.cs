using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TestOrderService.API.Commons;
using TestOrderService.API.Middleware;
using TestOrderService.Application.DTOs;
using TestOrderService.Application.Features.FeatureTestResults.Commands.SyncUpTestResults;
namespace TestOrderService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestResultsController(ISender sender) : ControllerBase
    {
        private readonly ISender _sender = sender;

        [HttpPost("sync-up")]
        [Authorize(Policy = "create_test_order")]
        [ProducesResponseType(typeof(ApiResponse<SyncUpTestResultsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> SyncUpTestResults(CancellationToken cancellationToken)
        {
            var updatedById = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(updatedById, out var updatedByIdGuid))
                throw new InvalidDataException($"Can not parse '{nameof(updatedById)}' to Guid: {updatedById}.");

            var command = new SyncUpTestResultsCommand(updatedByIdGuid);

            var testOrders = await _sender.Send(command, cancellationToken);
            var data = new SyncUpTestResultsResponse(testOrders);

            var response = ApiResponse<SyncUpTestResultsResponse>.Success(data, "Sync up test results successfully.");

            return Ok(response);
        }
    }
}
