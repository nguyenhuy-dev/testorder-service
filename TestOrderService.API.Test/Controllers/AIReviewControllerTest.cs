using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TestOrderService.API.Controllers;
using TestOrderService.API.Services;
using TestOrderService.Application.DTOs.AIReview;
using TestOrderService.Application.Interfaces;
using TestOrderService.Domain.Entities;
namespace TestOrderService.API.Test.Controllers
{
    [TestFixture]
    public class AIReviewControllerTest
    {

        [SetUp]
        public void Setup()
        {
            _mockAIReviewService = new Mock<IAIReviewService>();
            _mockLogger = new Mock<ILogger<AIReviewController>>();
            _mockTestOrderRepository = new Mock<ITestOrderRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _controller = new AIReviewController(
                _mockAIReviewService.Object,
                _mockLogger.Object,
                _mockTestOrderRepository.Object,
                _mockUnitOfWork.Object
            );
        }
        private Mock<IAIReviewService> _mockAIReviewService;
        private Mock<ILogger<AIReviewController>> _mockLogger;
        private Mock<ITestOrderRepository> _mockTestOrderRepository;
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private AIReviewController _controller;

        [Test]
        public async Task ReviewCBC_ShouldReturnOkAndUpdateStatus_WhenValidRequest()
        {
            // Arrange
            var testOrderId = Guid.NewGuid();
            var request = new AIReviewRequestDto { Sex = 0, Wbc = 5, Rbc = 4, Hgb = 13, Hct = 40, Plt = 200, Mcv = 90, Mch = 30, Mchc = 33 };
            var aiResult = new AIReviewResponseDto { Prediction = "Normal", PredictionCode = 0, Confidence = 99 };
            var testResults = new List<TestResult>
            {
                new TestResult { TestResultId = Guid.NewGuid(), Status = TestResultStatus.Completed },
                new TestResult { TestResultId = Guid.NewGuid(), Status = TestResultStatus.Completed }
            };
            var testOrder = new TestOrder { TestOrderId = testOrderId, Status = StatusTestOrder.Pending, TestResults = testResults };

            _mockAIReviewService.Setup(x => x.ReviewCBCAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync(aiResult);
            _mockTestOrderRepository.Setup(x => x.GetByIdAsync(testOrderId, It.IsAny<CancellationToken>())).ReturnsAsync(testOrder);
            _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _controller.ReviewCBC(request, testOrderId, CancellationToken.None);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.StatusCode, Is.EqualTo(200));
            Assert.That(((AIReviewResponseDto)okResult.Value!).Prediction, Is.EqualTo("Normal"));
            Assert.That(testOrder.Status, Is.EqualTo(StatusTestOrder.AIReviewed));
            Assert.That(testOrder.TestResults.TrueForAll(tr => tr.Status == TestResultStatus.AIReviewed));
        }

        [Test]
        public async Task ReviewCBC_ShouldReturnNotFound_WhenTestOrderNotFound()
        {
            // Arrange
            var testOrderId = Guid.NewGuid();
            var request = new AIReviewRequestDto();
            var aiResult = new AIReviewResponseDto();
            _mockAIReviewService.Setup(x => x.ReviewCBCAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync(aiResult);
            _mockTestOrderRepository.Setup(x => x.GetByIdAsync(testOrderId, It.IsAny<CancellationToken>())).ReturnsAsync((TestOrder?)null);

            // Act
            var result = await _controller.ReviewCBC(request, testOrderId, CancellationToken.None);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult, Is.Not.Null);
            Assert.That(notFoundResult!.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task ReviewCBC_ShouldReturn503_WhenAIServiceUnavailable()
        {
            // Arrange
            var testOrderId = Guid.NewGuid();
            var request = new AIReviewRequestDto();
            _mockAIReviewService.Setup(x => x.ReviewCBCAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync((AIReviewResponseDto?)null);

            // Act
            var result = await _controller.ReviewCBC(request, testOrderId, CancellationToken.None);

            // Assert
            var serviceUnavailable = result as ObjectResult;
            Assert.That(serviceUnavailable, Is.Not.Null);
            Assert.That(serviceUnavailable!.StatusCode, Is.EqualTo(503));
        }

        [Test]
        public async Task ReviewCBC_ShouldReturnBadRequest_WhenModelStateInvalid()
        {
            // Arrange
            var testOrderId = Guid.NewGuid();
            var request = new AIReviewRequestDto();
            _controller.ModelState.AddModelError("Wbc", "Required");

            // Act
            var result = await _controller.ReviewCBC(request, testOrderId, CancellationToken.None);

            // Assert
            var badRequest = result as BadRequestObjectResult;
            Assert.That(badRequest, Is.Not.Null);
            Assert.That(badRequest!.StatusCode, Is.EqualTo(400));
        }
    }
}
