using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using TestOrderService.API.Commons;
using TestOrderService.API.Controllers;
using TestOrderService.Application.DTOs;
using TestOrderService.Application.Features.MedicalTestResults.Commands.AddTestResultComment;
using TestOrderService.Application.Features.MedicalTestResults.Commands.DeleteTestResultComment;
using TestOrderService.Application.Features.MedicalTestResults.Commands.UpdateTestResultComment;
using UnauthorizedAccessException=TestOrderService.Application.Exceptions.UnauthorizedAccessException;

namespace TestOrderService.API.Test.Controllers
{
    [TestFixture]
    public class TestResultCommentsControllerTest
    {
        [SetUp]
        public void Setup()
        {
            _mockSender = new Mock<ISender>();
            _controller = new TestResultCommentsController(_mockSender.Object);
        }
        private Mock<ISender> _mockSender;
        private TestResultCommentsController _controller;

        /// <summary>
        ///     Helper to simulate a logged-in user by setting the ControllerContext.
        ///     This satisfies the BaseApiController.CurrentUserId logic.
        /// </summary>
        private void SetUserClaims(string userId, string name = "Test User", string role = "Doctor")
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Test]
        public async Task AddTestResultComment_ShouldReturnCreated_WhenRequestIsValid()
        {
            // Arrange
            var testResultId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var requestDto = new TestResultCommentRequestDto { Content = "Patient needs rest" };

            SetUserClaims(userId.ToString());

            var expectedResponse = new TestResultCommentDto
            {
                TestResultCommentId = Guid.NewGuid(),
                Content = requestDto.Content
            };

            _mockSender.Setup(x => x.Send(It.IsAny<AddTestResultCommentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.AddTestResultComment(testResultId, requestDto);

            // Assert
            var actionResult = result.Result as CreatedAtActionResult;
            Assert.That(actionResult, Is.Not.Null);
            Assert.That(actionResult!.StatusCode, Is.EqualTo(201));

            var apiResponse = actionResult.Value as ApiResponse<TestResultCommentDto>;
            Assert.That(apiResponse, Is.Not.Null);
            Assert.That(apiResponse!.Data!.Content, Is.EqualTo("Patient needs rest"));

            // Verify Mediator received the correct data from Claims and DTO
            _mockSender.Verify(x => x.Send(It.Is<AddTestResultCommentCommand>(c =>
                    c.TestResultId == testResultId &&
                    c.UserId == userId &&
                    c.Content == "Patient needs rest" &&
                    c.Name == "Test User" // From SetUserClaims helper
            ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void AddTestResultComment_ShouldThrowUnauthorized_WhenUserClaimIsInvalid()
        {
            // Arrange
            var testResultId = Guid.NewGuid();
            var requestDto = new TestResultCommentRequestDto { Content = "Test" };

            // Simulate invalid token (non-guid ID)
            SetUserClaims("not-a-guid");

            // Act & Assert
            var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _controller.AddTestResultComment(testResultId, requestDto));

            Assert.That(ex!.Message, Does.Contain("Authentication token is missing or invalid"));

            _mockSender.Verify(x => x.Send(It.IsAny<AddTestResultCommentCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task UpdateTestResultComment_ShouldReturnOk_WhenRequestIsValid()
        {
            // Arrange
            var testResultId = Guid.NewGuid();
            var commentId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var requestDto = new TestResultCommentRequestDto { Content = "Updated Content" };

            SetUserClaims(userId.ToString());

            var expectedResponse = new TestResultCommentDto { TestResultCommentId = commentId, Content = "Updated Content" };

            _mockSender.Setup(x => x.Send(It.IsAny<UpdateTestResultCommentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.UpdateTestResultComment(testResultId, commentId, requestDto);

            // Assert
            var actionResult = result.Result as OkObjectResult;
            Assert.That(actionResult, Is.Not.Null);
            Assert.That(actionResult!.StatusCode, Is.EqualTo(200));

            var apiResponse = actionResult.Value as ApiResponse<TestResultCommentDto>;
            Assert.That(apiResponse!.Data!.Content, Is.EqualTo("Updated Content"));

            // Verify Command mapping
            _mockSender.Verify(x => x.Send(It.Is<UpdateTestResultCommentCommand>(c =>
                c.CommentId == commentId &&
                c.TestResultId == testResultId &&
                c.UserId == userId
            ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void UpdateTestResultComment_ShouldThrowUnauthorized_WhenClaimsMissing()
        {
            // Arrange
            // Setup context with NO user
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act & Assert
            Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _controller.UpdateTestResultComment(Guid.NewGuid(), Guid.NewGuid(), new TestResultCommentRequestDto { Content = "X" }));
        }

        [Test]
        public async Task DeleteTestResultComment_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var testResultId = Guid.NewGuid();
            var commentId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            SetUserClaims(userId.ToString(), role: "Admin");

            _mockSender.Setup(x => x.Send(It.IsAny<DeleteTestResultCommentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteTestResultComment(testResultId, commentId);

            // Assert
            var actionResult = result.Result as OkObjectResult;
            Assert.That(actionResult, Is.Not.Null);
            Assert.That(actionResult!.StatusCode, Is.EqualTo(200));

            var apiResponse = actionResult.Value as ApiResponse<bool>;
            Assert.That(apiResponse!.Data, Is.True);

            // Verify Role was passed from claims
            _mockSender.Verify(x => x.Send(It.Is<DeleteTestResultCommentCommand>(c =>
                c.Role == "Admin" &&
                c.UserId == userId
            ), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
