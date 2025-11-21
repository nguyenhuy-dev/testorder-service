using FluentAssertions;
using NSubstitute;
using TestOrderService.Application.Exceptions;
using TestOrderService.Application.Features.TestOrders.Commands.UpdateComment;
using TestOrderService.Application.Interfaces;
using TestOrderService.Application.Interfaces.gRPC;
using TestOrderService.Domain.Entities;
using UnauthorizedAccessException=TestOrderService.Application.Exceptions.UnauthorizedAccessException;

namespace TestOrderService.Application.Test.Features.TestOrders.Commands.UpdateComment
{
    [TestFixture]
    public class UpdateCommentCommandHandlerTests
    {

        [SetUp]
        public void SetUp()
        {
            // Arrange - Create mocks
            _commentRepository = Substitute.For<ICommentRepository>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _userGrpcClient = Substitute.For<IUserGrpcClient>();

            // Create handler with mocked dependencies
            _handler = new UpdateCommentCommandHandler(
                _commentRepository,
                _unitOfWork,
                _userGrpcClient);
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up if needed
        }
        private ICommentRepository _commentRepository;
        private IUnitOfWork _unitOfWork;
        private IUserGrpcClient _userGrpcClient;
        private UpdateCommentCommandHandler _handler;

        [Test]
        public async Task Handle_WithValidRequest_ShouldUpdateCommentSuccessfully()
        {
            // Arrange
            var testOrderId = Guid.NewGuid();
            var commentId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var userName = "Dr. John Smith";
            var originalContent = "Original comment content";
            var updatedContent = "Updated comment content";

            var existingComment = new Comment
            {
                CommentId = commentId,
                TestOrderId = testOrderId,
                CreateById = userId,
                CreateByName = userName,
                Content = originalContent,
                CreateAt = DateTime.UtcNow.AddDays(-1)
            };

            var command = new UpdateCommentCommand
            {
                TestOrderId = testOrderId,
                CommentId = commentId,
                UserId = userId,
                Name = userName,
                Content = updatedContent
            };

            _commentRepository
                .GetCommentByIdAndTestOrderIdAsync(commentId, testOrderId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Comment?>(existingComment));

            _commentRepository
                .UpdateCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>())
                .Returns(callInfo => Task.FromResult(callInfo.ArgAt<Comment>(0)));

            _unitOfWork
                .SaveChangesAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(1));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.CommentId.Should().Be(commentId);
            result.Content.Should().Be(updatedContent);
            result.UpdateById.Should().Be(userId);
            result.UpdateByName.Should().Be(userName);
            result.UpdateAt.Should().NotBeNull();
            result.UpdateAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            result.CreateById.Should().Be(userId);
            result.CreateByName.Should().Be(userName);
            result.CreateAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(-1), TimeSpan.FromSeconds(5));

            // Verify interactions
            await _commentRepository
                .Received(1)
                .GetCommentByIdAndTestOrderIdAsync(commentId, testOrderId, Arg.Any<CancellationToken>());

            await _commentRepository
                .Received(1)
                .UpdateCommentAsync(
                    Arg.Is<Comment>(c =>
                        c.Content == updatedContent &&
                        c.UpdateById == userId &&
                        c.UpdateByName == userName &&
                        c.UpdateAt != null),
                    Arg.Any<CancellationToken>());

            await _unitOfWork
                .Received(1)
                .SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_WithValidRequest_ShouldSetUpdateAtToCurrentUtcTime()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingComment = new Comment
            {
                CommentId = Guid.NewGuid(),
                TestOrderId = Guid.NewGuid(),
                CreateById = userId,
                CreateByName = "Dr. Smith",
                Content = "Original content",
                CreateAt = DateTime.UtcNow.AddHours(-2)
            };

            var command = new UpdateCommentCommand
            {
                TestOrderId = existingComment.TestOrderId,
                CommentId = existingComment.CommentId,
                UserId = userId,
                Name = "Dr. Smith",
                Content = "Updated content"
            };

            _commentRepository
                .GetCommentByIdAndTestOrderIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(existingComment);

            _commentRepository
                .UpdateCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>())
                .Returns(callInfo => Task.FromResult(callInfo.ArgAt<Comment>(0)));

            _unitOfWork
                .SaveChangesAsync(Arg.Any<CancellationToken>())
                .Returns(1);

            var beforeExecution = DateTime.UtcNow;

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            var afterExecution = DateTime.UtcNow;

            // Assert
            result.UpdateAt.Should().NotBeNull();
            result.UpdateAt.Should().BeOnOrAfter(beforeExecution);
            result.UpdateAt.Should().BeOnOrBefore(afterExecution);
        }

        [Test]
        public async Task Handle_WithValidRequest_ShouldPreserveOriginalCreateData()
        {
            // Arrange
            var originalUserId = Guid.NewGuid();
            var updateUserId = Guid.NewGuid();
            var originalCreateAt = DateTime.UtcNow.AddDays(-5);

            var existingComment = new Comment
            {
                CommentId = Guid.NewGuid(),
                TestOrderId = Guid.NewGuid(),
                CreateById = originalUserId,
                CreateByName = "Dr. Original",
                Content = "Original content",
                CreateAt = originalCreateAt
            };

            var command = new UpdateCommentCommand
            {
                TestOrderId = existingComment.TestOrderId,
                CommentId = existingComment.CommentId,
                UserId = originalUserId, // Same user updating their own comment
                Name = "Dr. Original",
                Content = "Updated content"
            };

            _commentRepository
                .GetCommentByIdAndTestOrderIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(existingComment);

            _commentRepository
                .UpdateCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>())
                .Returns(callInfo => Task.FromResult(callInfo.ArgAt<Comment>(0)));

            _unitOfWork
                .SaveChangesAsync(Arg.Any<CancellationToken>())
                .Returns(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.CreateById.Should().Be(originalUserId);
            result.CreateByName.Should().Be("Dr. Original");
            result.CreateAt.Should().Be(originalCreateAt);
        }

        [Test]
        public async Task Handle_WithLongContent_ShouldAcceptContentUpTo2000Characters()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var longContent = new string('A', 2000);

            var existingComment = new Comment
            {
                CommentId = Guid.NewGuid(),
                TestOrderId = Guid.NewGuid(),
                CreateById = userId,
                CreateByName = "Dr. Smith",
                Content = "Original content",
                CreateAt = DateTime.UtcNow.AddDays(-1)
            };

            var command = new UpdateCommentCommand
            {
                TestOrderId = existingComment.TestOrderId,
                CommentId = existingComment.CommentId,
                UserId = userId,
                Name = "Dr. Smith",
                Content = longContent
            };

            _commentRepository
                .GetCommentByIdAndTestOrderIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(existingComment);

            _commentRepository
                .UpdateCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>())
                .Returns(callInfo => Task.FromResult(callInfo.ArgAt<Comment>(0)));

            _unitOfWork
                .SaveChangesAsync(Arg.Any<CancellationToken>())
                .Returns(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Content.Should().Be(longContent);
            result.Content.Length.Should().Be(2000);
        }

        [Test]
        public async Task Handle_WhenUserUpdatesOwnComment_ShouldSucceed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingComment = new Comment
            {
                CommentId = Guid.NewGuid(),
                TestOrderId = Guid.NewGuid(),
                CreateById = userId,
                CreateByName = "Dr. Smith",
                Content = "Original content",
                CreateAt = DateTime.UtcNow.AddDays(-1)
            };

            var command = new UpdateCommentCommand
            {
                TestOrderId = existingComment.TestOrderId,
                CommentId = existingComment.CommentId,
                UserId = userId, // Same user
                Name = "Dr. Smith",
                Content = "Updated by owner"
            };

            _commentRepository
                .GetCommentByIdAndTestOrderIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(existingComment);

            _commentRepository
                .UpdateCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>())
                .Returns(callInfo => Task.FromResult(callInfo.ArgAt<Comment>(0)));

            _unitOfWork
                .SaveChangesAsync(Arg.Any<CancellationToken>())
                .Returns(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Content.Should().Be("Updated by owner");
        }

        [Test]
        public void Handle_WhenCommentNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var commentId = Guid.NewGuid();
            var testOrderId = Guid.NewGuid();

            var command = new UpdateCommentCommand
            {
                TestOrderId = testOrderId,
                CommentId = commentId,
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = "Updated content"
            };

            _commentRepository
                .GetCommentByIdAndTestOrderIdAsync(commentId, testOrderId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Comment?>(null));

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"*{nameof(Comment)}*{commentId}*");

            // Verify UpdateCommentAsync was never called
            _commentRepository
                .DidNotReceive()
                .UpdateCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());

            _unitOfWork
                .DidNotReceive()
                .SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public void Handle_WhenUserIsNotOwner_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var originalUserId = Guid.NewGuid();
            var differentUserId = Guid.NewGuid();

            var existingComment = new Comment
            {
                CommentId = Guid.NewGuid(),
                TestOrderId = Guid.NewGuid(),
                CreateById = originalUserId,
                CreateByName = "Dr. Original",
                Content = "Original content",
                CreateAt = DateTime.UtcNow.AddDays(-1)
            };

            var command = new UpdateCommentCommand
            {
                TestOrderId = existingComment.TestOrderId,
                CommentId = existingComment.CommentId,
                UserId = differentUserId, // Different user
                Name = "Dr. Different",
                Content = "Trying to update someone else's comment"
            };

            _commentRepository
                .GetCommentByIdAndTestOrderIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(existingComment);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("User cannot update other user's comments");

            // Verify UpdateCommentAsync was never called
            _commentRepository
                .DidNotReceive()
                .UpdateCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());

            _unitOfWork
                .DidNotReceive()
                .SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public void Handle_WhenRepositoryGetThrowsException_ShouldPropagateException()
        {
            // Arrange
            var command = new UpdateCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                CommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = "Updated content"
            };

            _commentRepository
                .GetCommentByIdAndTestOrderIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromException<Comment?>(new InvalidOperationException("Database error")));

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Database error");
        }

        [Test]
        public void Handle_WhenRepositoryUpdateFails_ShouldPropagateException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingComment = new Comment
            {
                CommentId = Guid.NewGuid(),
                TestOrderId = Guid.NewGuid(),
                CreateById = userId,
                CreateByName = "Dr. Smith",
                Content = "Original content",
                CreateAt = DateTime.UtcNow.AddDays(-1)
            };

            var command = new UpdateCommentCommand
            {
                TestOrderId = existingComment.TestOrderId,
                CommentId = existingComment.CommentId,
                UserId = userId,
                Name = "Dr. Smith",
                Content = "Updated content"
            };

            _commentRepository
                .GetCommentByIdAndTestOrderIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(existingComment);

            _commentRepository
                .UpdateCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromException<Comment>(new InvalidOperationException("Update failed")));

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Update failed");

            // Verify SaveChanges was never called
            _unitOfWork
                .DidNotReceive()
                .SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public void Handle_WhenUnitOfWorkSaveFails_ShouldPropagateException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingComment = new Comment
            {
                CommentId = Guid.NewGuid(),
                TestOrderId = Guid.NewGuid(),
                CreateById = userId,
                CreateByName = "Dr. Smith",
                Content = "Original content",
                CreateAt = DateTime.UtcNow.AddDays(-1)
            };

            var command = new UpdateCommentCommand
            {
                TestOrderId = existingComment.TestOrderId,
                CommentId = existingComment.CommentId,
                UserId = userId,
                Name = "Dr. Smith",
                Content = "Updated content"
            };

            _commentRepository
                .GetCommentByIdAndTestOrderIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(existingComment);

            _commentRepository
                .UpdateCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>())
                .Returns(callInfo => Task.FromResult(callInfo.ArgAt<Comment>(0)));

            _unitOfWork
                .SaveChangesAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromException<int>(new InvalidOperationException("Save failed")));

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Save failed");
        }

        [Test]
        public async Task Handle_WithCancellationToken_ShouldPassTokenToRepositoryMethods()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingComment = new Comment
            {
                CommentId = Guid.NewGuid(),
                TestOrderId = Guid.NewGuid(),
                CreateById = userId,
                CreateByName = "Dr. Smith",
                Content = "Original content",
                CreateAt = DateTime.UtcNow.AddDays(-1)
            };

            var command = new UpdateCommentCommand
            {
                TestOrderId = existingComment.TestOrderId,
                CommentId = existingComment.CommentId,
                UserId = userId,
                Name = "Dr. Smith",
                Content = "Updated content"
            };

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            _commentRepository
                .GetCommentByIdAndTestOrderIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), cancellationToken)
                .Returns(existingComment);

            _commentRepository
                .UpdateCommentAsync(Arg.Any<Comment>(), cancellationToken)
                .Returns(callInfo => Task.FromResult(callInfo.ArgAt<Comment>(0)));

            _unitOfWork
                .SaveChangesAsync(cancellationToken)
                .Returns(1);

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();

            await _commentRepository
                .Received(1)
                .GetCommentByIdAndTestOrderIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), cancellationToken);

            await _commentRepository
                .Received(1)
                .UpdateCommentAsync(Arg.Any<Comment>(), cancellationToken);

            await _unitOfWork
                .Received(1)
                .SaveChangesAsync(cancellationToken);
        }

        [Test]
        public async Task Handle_WithMinimalContent_ShouldUpdateCommentSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingComment = new Comment
            {
                CommentId = Guid.NewGuid(),
                TestOrderId = Guid.NewGuid(),
                CreateById = userId,
                CreateByName = "Dr. Smith",
                Content = "Original content",
                CreateAt = DateTime.UtcNow.AddDays(-1)
            };

            var command = new UpdateCommentCommand
            {
                TestOrderId = existingComment.TestOrderId,
                CommentId = existingComment.CommentId,
                UserId = userId,
                Name = "Dr. Smith",
                Content = "A" // Single character
            };

            _commentRepository
                .GetCommentByIdAndTestOrderIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(existingComment);

            _commentRepository
                .UpdateCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>())
                .Returns(callInfo => Task.FromResult(callInfo.ArgAt<Comment>(0)));

            _unitOfWork
                .SaveChangesAsync(Arg.Any<CancellationToken>())
                .Returns(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Content.Should().Be("A");
        }

        [Test]
        public async Task Handle_WithSpecialCharactersInContent_ShouldPreserveContent()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var specialContent = "Test with special chars: @#$%^&*()_+-=[]{}|;':\"<>?,./";

            var existingComment = new Comment
            {
                CommentId = Guid.NewGuid(),
                TestOrderId = Guid.NewGuid(),
                CreateById = userId,
                CreateByName = "Dr. Smith",
                Content = "Original content",
                CreateAt = DateTime.UtcNow.AddDays(-1)
            };

            var command = new UpdateCommentCommand
            {
                TestOrderId = existingComment.TestOrderId,
                CommentId = existingComment.CommentId,
                UserId = userId,
                Name = "Dr. Smith",
                Content = specialContent
            };

            _commentRepository
                .GetCommentByIdAndTestOrderIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(existingComment);

            _commentRepository
                .UpdateCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>())
                .Returns(callInfo => Task.FromResult(callInfo.ArgAt<Comment>(0)));

            _unitOfWork
                .SaveChangesAsync(Arg.Any<CancellationToken>())
                .Returns(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Content.Should().Be(specialContent);
        }

        [Test]
        public async Task Handle_WithUnicodeCharactersInContent_ShouldPreserveContent()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var unicodeContent = "Test with unicode: 你好世界 مرحبا العالم שלום עולם";

            var existingComment = new Comment
            {
                CommentId = Guid.NewGuid(),
                TestOrderId = Guid.NewGuid(),
                CreateById = userId,
                CreateByName = "Dr. Smith",
                Content = "Original content",
                CreateAt = DateTime.UtcNow.AddDays(-1)
            };

            var command = new UpdateCommentCommand
            {
                TestOrderId = existingComment.TestOrderId,
                CommentId = existingComment.CommentId,
                UserId = userId,
                Name = "Dr. Smith",
                Content = unicodeContent
            };

            _commentRepository
                .GetCommentByIdAndTestOrderIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(existingComment);

            _commentRepository
                .UpdateCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>())
                .Returns(callInfo => Task.FromResult(callInfo.ArgAt<Comment>(0)));

            _unitOfWork
                .SaveChangesAsync(Arg.Any<CancellationToken>())
                .Returns(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Content.Should().Be(unicodeContent);
        }

        [Test]
        public async Task Handle_WhenCommentHasBeenPreviouslyUpdated_ShouldOverwriteUpdateData()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var previousUpdateTime = DateTime.UtcNow.AddHours(-2);

            var existingComment = new Comment
            {
                CommentId = Guid.NewGuid(),
                TestOrderId = Guid.NewGuid(),
                CreateById = userId,
                CreateByName = "Dr. Smith",
                Content = "Previously updated content",
                CreateAt = DateTime.UtcNow.AddDays(-5),
                UpdateById = userId,
                UpdateByName = "Dr. Smith",
                UpdateAt = previousUpdateTime
            };

            var command = new UpdateCommentCommand
            {
                TestOrderId = existingComment.TestOrderId,
                CommentId = existingComment.CommentId,
                UserId = userId,
                Name = "Dr. Smith",
                Content = "Newly updated content"
            };

            _commentRepository
                .GetCommentByIdAndTestOrderIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(existingComment);

            _commentRepository
                .UpdateCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>())
                .Returns(callInfo => Task.FromResult(callInfo.ArgAt<Comment>(0)));

            _unitOfWork
                .SaveChangesAsync(Arg.Any<CancellationToken>())
                .Returns(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Content.Should().Be("Newly updated content");
            result.UpdateAt.Should().BeAfter(previousUpdateTime);
        }

        [Test]
        public async Task Handle_ShouldCallRepositoryMethodsInCorrectOrder()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingComment = new Comment
            {
                CommentId = Guid.NewGuid(),
                TestOrderId = Guid.NewGuid(),
                CreateById = userId,
                CreateByName = "Dr. Smith",
                Content = "Original content",
                CreateAt = DateTime.UtcNow.AddDays(-1)
            };

            var command = new UpdateCommentCommand
            {
                TestOrderId = existingComment.TestOrderId,
                CommentId = existingComment.CommentId,
                UserId = userId,
                Name = "Dr. Smith",
                Content = "Updated content"
            };

            var callOrder = new List<string>();

            _commentRepository
                .GetCommentByIdAndTestOrderIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(callInfo =>
                {
                    callOrder.Add("GetCommentByIdAndTestOrderIdAsync");
                    return Task.FromResult<Comment?>(existingComment);
                });

            _commentRepository
                .UpdateCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>())
                .Returns(callInfo =>
                {
                    callOrder.Add("UpdateCommentAsync");
                    return Task.FromResult(callInfo.ArgAt<Comment>(0));
                });

            _unitOfWork
                .SaveChangesAsync(Arg.Any<CancellationToken>())
                .Returns(callInfo =>
                {
                    callOrder.Add("SaveChangesAsync");
                    return Task.FromResult(1);
                });

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            callOrder.Should().ContainInOrder(
                "GetCommentByIdAndTestOrderIdAsync",
                "UpdateCommentAsync",
                "SaveChangesAsync"
            );
        }

        [Test]
        public void Handle_WithEmptyUserId_ShouldStillPerformOwnershipCheck()
        {
            // Arrange
            var existingComment = new Comment
            {
                CommentId = Guid.NewGuid(),
                TestOrderId = Guid.NewGuid(),
                CreateById = Guid.NewGuid(),
                CreateByName = "Dr. Smith",
                Content = "Original content",
                CreateAt = DateTime.UtcNow.AddDays(-1)
            };

            var command = new UpdateCommentCommand
            {
                TestOrderId = existingComment.TestOrderId,
                CommentId = existingComment.CommentId,
                UserId = Guid.Empty, // Empty GUID
                Name = "Dr. Hacker",
                Content = "Trying to update"
            };

            _commentRepository
                .GetCommentByIdAndTestOrderIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(existingComment);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("User cannot update other user's comments");
        }

        [Test]
        public async Task Handle_MultipleUpdatesToSameComment_ShouldSucceed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var commentId = Guid.NewGuid();
            var testOrderId = Guid.NewGuid();

            var existingComment = new Comment
            {
                CommentId = commentId,
                TestOrderId = testOrderId,
                CreateById = userId,
                CreateByName = "Dr. Smith",
                Content = "Original content",
                CreateAt = DateTime.UtcNow.AddDays(-1)
            };

            _commentRepository
                .GetCommentByIdAndTestOrderIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(existingComment);

            _commentRepository
                .UpdateCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>())
                .Returns(callInfo => Task.FromResult(callInfo.ArgAt<Comment>(0)));

            _unitOfWork
                .SaveChangesAsync(Arg.Any<CancellationToken>())
                .Returns(1);

            // First update
            var command1 = new UpdateCommentCommand
            {
                TestOrderId = testOrderId,
                CommentId = commentId,
                UserId = userId,
                Name = "Dr. Smith",
                Content = "First update"
            };

            var result1 = await _handler.Handle(command1, CancellationToken.None);

            // Second update
            var command2 = new UpdateCommentCommand
            {
                TestOrderId = testOrderId,
                CommentId = commentId,
                UserId = userId,
                Name = "Dr. Smith",
                Content = "Second update"
            };

            var result2 = await _handler.Handle(command2, CancellationToken.None);

            // Assert
            result1.Content.Should().Be("First update");
            result2.Content.Should().Be("Second update");
            result2.UpdateAt.Should().BeOnOrAfter(result1.UpdateAt!.Value);
        }
    }
}
