using FluentAssertions;
using NSubstitute;
using TestOrderService.Application.Exceptions;
using TestOrderService.Application.Features.MedicalTestResults.Commands.UpdateTestResultComment;
using TestOrderService.Application.Interfaces;
using TestOrderService.Application.Interfaces.gRPC;
using TestOrderService.Domain.Entities;
using UnauthorizedAccessException=TestOrderService.Application.Exceptions.UnauthorizedAccessException;

namespace TestOrderService.Application.Test.Features.MedicalTestResults.Commands.UpdateTestResultComment
{
    [TestFixture]
    public class UpdateTestResultCommentCommandHandlerTests
    {
        [SetUp]
        public void SetUp()
        {
            _testResultCommentRepository = Substitute.For<ITestResultCommentRepository>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _userGrpcClient = Substitute.For<IUserGrpcClient>();

            _handler = new UpdateTestResultCommentCommandHandler(
                _testResultCommentRepository,
                _unitOfWork,
                _userGrpcClient);
        }

        private ITestResultCommentRepository _testResultCommentRepository;
        private IUnitOfWork _unitOfWork;
        private IUserGrpcClient _userGrpcClient;
        private UpdateTestResultCommentCommandHandler _handler;

        [Test]
        public async Task Handle_WithValidRequest_ShouldUpdateCommentSuccessfully()
        {
            // Arrange
            var commentId = Guid.NewGuid();
            var testResultId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var userName = "Dr. John Smith";
            var content = "Updated content";

            var command = new UpdateTestResultCommentCommand
            {
                CommentId = commentId,
                TestResultId = testResultId,
                UserId = userId,
                Name = userName,
                Content = content
            };

            var existingComment = new TestResultComment
            {
                TestResultCommentId = commentId,
                TestResultId = testResultId,
                CreateById = userId, // Same user
                Content = "Old content"
            };

            _testResultCommentRepository
                .GetTestResultCommentByIdAndTestResultIdAsync(commentId, testResultId, Arg.Any<CancellationToken>())
                .Returns(existingComment);

            _unitOfWork
                .SaveChangesAsync(Arg.Any<CancellationToken>())
                .Returns(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Content.Should().Be(content);

            await _testResultCommentRepository
                .Received(1)
                .UpdateTestResultCommentAsync(
                    Arg.Is<TestResultComment>(c => c.Content == content && c.UpdateById == userId),
                    Arg.Any<CancellationToken>());

            await _unitOfWork
                .Received(1)
                .SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public void Handle_WhenCommentNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var command = new UpdateTestResultCommentCommand
            {
                CommentId = Guid.NewGuid(),
                TestResultId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Content = "Updated content"
            };

            _testResultCommentRepository
                .GetTestResultCommentByIdAndTestResultIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns((TestResultComment)null!);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"*{nameof(TestResultComment)}*{command.CommentId}*");
        }

        [Test]
        public void Handle_WhenUserIsNotOwner_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var command = new UpdateTestResultCommentCommand
            {
                CommentId = Guid.NewGuid(),
                TestResultId = Guid.NewGuid(),
                UserId = userId,
                Content = "Updated content"
            };

            var existingComment = new TestResultComment
            {
                TestResultCommentId = command.CommentId,
                TestResultId = command.TestResultId,
                CreateById = ownerId // Different user
            };

            _testResultCommentRepository
                .GetTestResultCommentByIdAndTestResultIdAsync(command.CommentId, command.TestResultId, Arg.Any<CancellationToken>())
                .Returns(existingComment);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("User cannot update other user's comments");
        }
    }
}
