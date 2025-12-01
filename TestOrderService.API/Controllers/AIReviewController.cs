using Microsoft.AspNetCore.Mvc;
using TestOrderService.API.Services;
using TestOrderService.Application.DTOs.AIReview;
using TestOrderService.Application.Interfaces;
namespace TestOrderService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIReviewController : ControllerBase
    {
        private readonly IAIReviewService _aiReviewService;
        private readonly ILogger<AIReviewController> _logger;
        private readonly ITestOrderRepository _testOrderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AIReviewController(
            IAIReviewService aiReviewService,
            ILogger<AIReviewController> logger,
            ITestOrderRepository testOrderRepository,
            IUnitOfWork unitOfWork)
        {
            _aiReviewService = aiReviewService;
            _logger = logger;
            _testOrderRepository = testOrderRepository;
            _unitOfWork = unitOfWork;
        }


        /// <summary>
        ///     Get AI-powered review for CBC test results and update statuses
        /// </summary>
        /// <param name="request">CBC test parameters</param>
        /// <param name="testOrderId">TestOrderId to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>AI review result with prediction, confidence, and suggestions</returns>
        [HttpPost("review-cbc")]
        [ProducesResponseType(typeof(AIReviewResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> ReviewCBC(
            [FromBody] AIReviewRequestDto request,
            [FromQuery] Guid testOrderId,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Received AI review request for CBC test");

            var result = await _aiReviewService.ReviewCBCAsync(request, cancellationToken);

            if (result == null)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new { message = "AI Review Service is currently unavailable" });
            }

            var testOrder = await _testOrderRepository.GetByIdAsync(testOrderId, cancellationToken);
            if (testOrder == null)
            {
                return NotFound(new { message = "TestOrder not found" });
            }

            return Ok(result);
        }
    }
}
