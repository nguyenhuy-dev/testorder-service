using FluentAssertions;
using NSubstitute;
using TestOrderService.Application.Exceptions;
using TestOrderService.Application.Features.MedicalTestResults.Commands.DeleteTestResultComment;
using TestOrderService.Application.Interfaces;
using TestOrderService.Application.Interfaces.gRPC;
using TestOrderService.Domain.Entities;
using UnauthorizedAccessException=TestOrderService.Application.Exceptions.UnauthorizedAccessException;

namespace TestOrderService.Application.Test.Features.MedicalTestResults.Commands.DeleteTestResultComment
{
    [TestFixture]
    public class DeleteTestResultCommentCommandHandlerTests
    {
        [SetUp]
        public void SetUp()
        {
            _testResultCommentRepository = Substitute.For<ITestResultCommentRepository>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _userGrpcClient = Substitute.For<IUserGrpcClient>();

            _handler = new DeleteTestResultCommentCommandHandler(
                _testResultCommentRepository,
                _unitOfWork,
                _userGrpcClient);
        }

        private ITestResultCommentRepository _testResultCommentRepository;
        private IUnitOfWork _unitOfWork;
        private IUserGrpcClient _userGrpcClient;
        private DeleteTestResultCommentCommandHandler _handler;

        [Test]
        public async Task Handle_WhenUserIsOwner_ShouldDeleteComment()
        {
            // Arrange
            var commentId = Guid.NewGuid();
            var testResultId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var command = new DeleteTestResultCommentCommand
            {
                CommentId = commentId,
                TestResultId = testResultId,
                UserId = userId,
                Role = "Doctor"
            };

            var existingComment = new TestResultComment
            {
                TestResultCommentId = commentId,
                TestResultId = testResultId,
                CreateById = userId // Same user
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
            result.Should().BeTrue();

            await _testResultCommentRepository
                .Received(1)
                .DeleteTestResultCommentAsync(existingComment, Arg.Any<CancellationToken>());

            await _unitOfWork
                .Received(1)
                .SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_WhenUserIsAdmin_ShouldDeleteComment()
        {
            // Arrange
            var commentId = Guid.NewGuid();
            var testResultId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();

            var command = new DeleteTestResultCommentCommand
            {
                CommentId = commentId,
                TestResultId = testResultId,
                UserId = userId,
                Role = "admin" // Admin role
            };

            var existingComment = new TestResultComment
            {
                TestResultCommentId = commentId,
                TestResultId = testResultId,
                CreateById = ownerId // Different user
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
            result.Should().BeTrue();

            await _testResultCommentRepository
                .Received(1)
                .DeleteTestResultCommentAsync(existingComment, Arg.Any<CancellationToken>());
        }

        [Test]
        public void Handle_WhenUserIsNotOwnerAndNotAdmin_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var commentId = Guid.NewGuid();
            var testResultId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();

            var command = new DeleteTestResultCommentCommand
            {
                CommentId = commentId,
                TestResultId = testResultId,
                UserId = userId,
                Role = "Doctor"
            };

            var existingComment = new TestResultComment
            {
                TestResultCommentId = commentId,
                TestResultId = testResultId,
                CreateById = ownerId // Different user
            };

            _testResultCommentRepository
                .GetTestResultCommentByIdAndTestResultIdAsync(commentId, testResultId, Arg.Any<CancellationToken>())
                .Returns(existingComment);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("User cannot delete other user's comments");
        }

        [Test]
        public void Handle_WhenCommentNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var command = new DeleteTestResultCommentCommand
            {
                CommentId = Guid.NewGuid(),
                TestResultId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
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
    }
}
