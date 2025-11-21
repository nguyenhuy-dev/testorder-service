using FluentAssertions;
using NSubstitute;
using TestOrderService.Application.Exceptions;
using TestOrderService.Application.Features.TestOrders.Commands.DeleteComment;
using TestOrderService.Application.Interfaces;
using TestOrderService.Application.Interfaces.gRPC;
using TestOrderService.Domain.Entities;
using UnauthorizedAccessException=TestOrderService.Application.Exceptions.UnauthorizedAccessException;

namespace TestOrderService.Application.Test.Features.TestOrders.Commands.DeleteComment
{
    [TestFixture]
    public class DeleteCommentCommandHandlerTests
    {

        [SetUp]
        public void SetUp()
        {
            // Arrange - Create mocks
            _commentRepository = Substitute.For<ICommentRepository>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _userGrpcClient = Substitute.For<IUserGrpcClient>();

            // Create handler with mocked dependencies
            _handler = new DeleteCommentCommandHandler(
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
        private DeleteCommentCommandHandler _handler;

        [Test]
        public async Task Handle_WhenOwnerDeletesOwnComment_ShouldDeleteSuccessfully()
        {
            // Arrange
            var testOrderId = Guid.NewGuid();
            var commentId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var userName = "Dr. John Smith";

            var existingComment = new Comment
            {
                CommentId = commentId,
                TestOrderId = testOrderId,
                CreateById = userId,
                CreateByName = userName,
                Content = "Comment to be deleted",
                CreateAt = DateTime.UtcNow.AddDays(-1)
            };

            var command = new DeleteCommentCommand
            {
                TestOrderId = testOrderId,
                CommentId = commentId,
                UserId = userId,
                Role = "doctor"
            };

            _commentRepository
                .GetCommentByIdAndTestOrderIdAsync(commentId, testOrderId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Comment?>(existingComment));

            _commentRepository
                .DeleteCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            _unitOfWork
                .SaveChangesAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(1));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();

            // Verify interactions
            await _commentRepository
                .Received(1)
                .GetCommentByIdAndTestOrderIdAsync(commentId, testOrderId, Arg.Any<CancellationToken>());

            await _commentRepository
                .Received(1)
                .DeleteCommentAsync(
                    Arg.Is<Comment>(c => c.CommentId == commentId),
                    Arg.Any<CancellationToken>());

            await _unitOfWork
                .Received(1)
                .SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_WhenAdminDeletesAnyComment_ShouldDeleteSuccessfully()
        {
            // Arrange
            var testOrderId = Guid.NewGuid();
            var commentId = Guid.NewGuid();
            var commentOwnerId = Guid.NewGuid();
            var adminUserId = Guid.NewGuid(); // Different user

            var existingComment = new Comment
            {
                CommentId = commentId,
                TestOrderId = testOrderId,
                CreateById = commentOwnerId,
                CreateByName = "Dr. Owner",
                Content = "Comment to be deleted",
                CreateAt = DateTime.UtcNow.AddDays(-1)
            };

            var command = new DeleteCommentCommand
            {
                TestOrderId = testOrderId,
                CommentId = commentId,
                UserId = adminUserId,
                Role = "admin" // Admin role
            };

            _commentRepository
                .GetCommentByIdAndTestOrderIdAsync(commentId, testOrderId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Comment?>(existingComment));

            _commentRepository
                .DeleteCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            _unitOfWork
                .SaveChangesAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(1));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();

            await _commentRepository
                .Received(1)
                .DeleteCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_WhenAdminWithMixedCaseRole_ShouldDeleteSuccessfully()
        {
            // Arrange
            var testOrderId = Guid.NewGuid();
            var commentId = Guid.NewGuid();
            var commentOwnerId = Guid.NewGuid();
            var adminUserId = Guid.NewGuid();

            var existingComment = new Comment
            {
                CommentId = commentId,
                TestOrderId = testOrderId,
                CreateById = commentOwnerId,
                CreateByName = "Dr. Owner",
                Content = "Comment to be deleted",
                CreateAt = DateTime.UtcNow.AddDays(-1)
            };

            var command = new DeleteCommentCommand
            {
                TestOrderId = testOrderId,
                CommentId = commentId,
                UserId = adminUserId,
                Role = "admin" // All caps
            };

            _commentRepository
                .GetCommentByIdAndTestOrderIdAsync(commentId, testOrderId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Comment?>(existingComment));

            _commentRepository
                .DeleteCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            _unitOfWork
                .SaveChangesAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(1));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void Handle_WhenCommentNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var commentId = Guid.NewGuid();
            var testOrderId = Guid.NewGuid();

            var command = new DeleteCommentCommand
            {
                TestOrderId = testOrderId,
                CommentId = commentId,
                UserId = Guid.NewGuid(),
                Role = "doctor"
            };

            _commentRepository
                .GetCommentByIdAndTestOrderIdAsync(commentId, testOrderId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Comment?>(null));

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"*{nameof(Comment)}*{commentId}*");

            // Verify DeleteCommentAsync was never called
            _commentRepository
                .DidNotReceive()
                .DeleteCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());

            _unitOfWork
                .DidNotReceive()
                .SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public void Handle_WhenNonOwnerNonAdminAttemptsDelete_ShouldThrowUnauthorizedAccessException()
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
                Content = "Original comment",
                CreateAt = DateTime.UtcNow.AddDays(-1)
            };

            var command = new DeleteCommentCommand
            {
                TestOrderId = existingComment.TestOrderId,
                CommentId = existingComment.CommentId,
                UserId = differentUserId, // Different user
                Role = "doctor" // Not admin
            };

            _commentRepository
                .GetCommentByIdAndTestOrderIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(existingComment);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("User cannot delete other user's comments");

            // Verify DeleteCommentAsync was never called
            _commentRepository
                .DidNotReceive()
                .DeleteCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());

            _unitOfWork
                .DidNotReceive()
                .SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public void Handle_WhenDoctorTriesToDeleteOtherDoctorComment_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var doctor1Id = Guid.NewGuid();
            var doctor2Id = Guid.NewGuid();

            var existingComment = new Comment
            {
                CommentId = Guid.NewGuid(),
                TestOrderId = Guid.NewGuid(),
                CreateById = doctor1Id,
                CreateByName = "Dr. One",
                Content = "Doctor 1's comment",
                CreateAt = DateTime.UtcNow.AddDays(-1)
            };

            var command = new DeleteCommentCommand
            {
                TestOrderId = existingComment.TestOrderId,
                CommentId = existingComment.CommentId,
                UserId = doctor2Id,
                Role = "doctor"
            };

            _commentRepository
                .GetCommentByIdAndTestOrderIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(existingComment);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("User cannot delete other user's comments");
        }

        [Test]
        public void Handle_WhenTechnicianTriesToDeleteOtherComment_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var technicianId = Guid.NewGuid();

            var existingComment = new Comment
            {
                CommentId = Guid.NewGuid(),
                TestOrderId = Guid.NewGuid(),
                CreateById = ownerId,
                CreateByName = "Dr. Owner",
                Content = "Owner's comment",
                CreateAt = DateTime.UtcNow.AddDays(-1)
            };

            var command = new DeleteCommentCommand
            {
                TestOrderId = existingComment.TestOrderId,
                CommentId = existingComment.CommentId,
                UserId = technicianId,
                Role = "technician"
            };

            _commentRepository
                .GetCommentByIdAndTestOrderIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(existingComment);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Test]
        public void Handle_WhenRepositoryGetThrowsException_ShouldPropagateException()
        {
            // Arrange
            var command = new DeleteCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                CommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Role = "doctor"
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
        public void Handle_WhenRepositoryDeleteFails_ShouldPropagateException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingComment = new Comment
            {
                CommentId = Guid.NewGuid(),
                TestOrderId = Guid.NewGuid(),
                CreateById = userId,
                CreateByName = "Dr. Smith",
                Content = "Comment to delete",
                CreateAt = DateTime.UtcNow.AddDays(-1)
            };

            var command = new DeleteCommentCommand
            {
                TestOrderId = existingComment.TestOrderId,
                CommentId = existingComment.CommentId,
                UserId = userId,
                Role = "doctor"
            };

            _commentRepository
                .GetCommentByIdAndTestOrderIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(existingComment);

            _commentRepository
                .DeleteCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromException(new InvalidOperationException("Delete failed")));

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Delete failed");

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
                Content = "Comment to delete",
                CreateAt = DateTime.UtcNow.AddDays(-1)
            };

            var command = new DeleteCommentCommand
            {
                TestOrderId = existingComment.TestOrderId,
                CommentId = existingComment.CommentId,
                UserId = userId,
                Role = "doctor"
            };

            _commentRepository
                .GetCommentByIdAndTestOrderIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(existingComment);

            _commentRepository
                .DeleteCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

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
                Content = "Comment to delete",
                CreateAt = DateTime.UtcNow.AddDays(-1)
            };

            var command = new DeleteCommentCommand
            {
                TestOrderId = existingComment.TestOrderId,
                CommentId = existingComment.CommentId,
                UserId = userId,
                Role = "doctor"
            };

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            _commentRepository
                .GetCommentByIdAndTestOrderIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), cancellationToken)
                .Returns(existingComment);

            _commentRepository
                .DeleteCommentAsync(Arg.Any<Comment>(), cancellationToken)
                .Returns(Task.CompletedTask);

            _unitOfWork
                .SaveChangesAsync(cancellationToken)
                .Returns(1);

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().BeTrue();

            await _commentRepository
                .Received(1)
                .GetCommentByIdAndTestOrderIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), cancellationToken);

            await _commentRepository
                .Received(1)
                .DeleteCommentAsync(Arg.Any<Comment>(), cancellationToken);

            await _unitOfWork
                .Received(1)
                .SaveChangesAsync(cancellationToken);
        }

        [Test]
        public void Handle_WithEmptyUserId_ShouldStillPerformAuthorizationCheck()
        {
            // Arrange
            var existingComment = new Comment
            {
                CommentId = Guid.NewGuid(),
                TestOrderId = Guid.NewGuid(),
                CreateById = Guid.NewGuid(),
                CreateByName = "Dr. Smith",
                Content = "Comment to delete",
                CreateAt = DateTime.UtcNow.AddDays(-1)
            };

            var command = new DeleteCommentCommand
            {
                TestOrderId = existingComment.TestOrderId,
                CommentId = existingComment.CommentId,
                UserId = Guid.Empty, // Empty GUID
                Role = "doctor"
            };

            _commentRepository
                .GetCommentByIdAndTestOrderIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(existingComment);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("User cannot delete other user's comments");
        }

        [Test]
        public async Task Handle_WhenCommentWasPreviouslyUpdated_ShouldStillDeleteSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingComment = new Comment
            {
                CommentId = Guid.NewGuid(),
                TestOrderId = Guid.NewGuid(),
                CreateById = userId,
                CreateByName = "Dr. Smith",
                Content = "Updated comment",
                CreateAt = DateTime.UtcNow.AddDays(-5),
                UpdateById = userId,
                UpdateByName = "Dr. Smith",
                UpdateAt = DateTime.UtcNow.AddHours(-2)
            };

            var command = new DeleteCommentCommand
            {
                TestOrderId = existingComment.TestOrderId,
                CommentId = existingComment.CommentId,
                UserId = userId,
                Role = "doctor"
            };

            _commentRepository
                .GetCommentByIdAndTestOrderIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(existingComment);

            _commentRepository
                .DeleteCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            _unitOfWork
                .SaveChangesAsync(Arg.Any<CancellationToken>())
                .Returns(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
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
                Content = "Comment to delete",
                CreateAt = DateTime.UtcNow.AddDays(-1)
            };

            var command = new DeleteCommentCommand
            {
                TestOrderId = existingComment.TestOrderId,
                CommentId = existingComment.CommentId,
                UserId = userId,
                Role = "doctor"
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
                .DeleteCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>())
                .Returns(callInfo =>
                {
                    callOrder.Add("DeleteCommentAsync");
                    return Task.CompletedTask;
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
                "DeleteCommentAsync",
                "SaveChangesAsync"
            );
        }

        [Test]
        [TestCase("admin", true, Description = "Admin can delete any comment")]
        [TestCase("Admin", true, Description = "Admin with capital A can delete")]
        [TestCase("ADMIN", true, Description = "ADMIN all caps can delete")]
        [TestCase("doctor", false, Description = "Doctor cannot delete others' comments")]
        [TestCase("Doctor", false, Description = "Doctor with capital D cannot delete")]
        [TestCase("technician", false, Description = "Technician cannot delete others' comments")]
        [TestCase("Technician", false, Description = "Technician with capital T cannot delete")]
        [TestCase("nurse", false, Description = "Nurse cannot delete others' comments")]
        [TestCase("user", false, Description = "Regular user cannot delete others' comments")]
        public void Handle_WithVariousRoles_ShouldEnforceAuthorizationCorrectly(
            string role,
            bool shouldSucceed)
        {
            // Arrange
            var commentOwnerId = Guid.NewGuid();
            var differentUserId = Guid.NewGuid();

            var existingComment = new Comment
            {
                CommentId = Guid.NewGuid(),
                TestOrderId = Guid.NewGuid(),
                CreateById = commentOwnerId,
                CreateByName = "Dr. Owner",
                Content = "Owner's comment",
                CreateAt = DateTime.UtcNow.AddDays(-1)
            };

            var command = new DeleteCommentCommand
            {
                TestOrderId = existingComment.TestOrderId,
                CommentId = existingComment.CommentId,
                UserId = differentUserId, // Different user trying to delete
                Role = role
            };

            _commentRepository
                .GetCommentByIdAndTestOrderIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(existingComment);

            _commentRepository
                .DeleteCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            _unitOfWork
                .SaveChangesAsync(Arg.Any<CancellationToken>())
                .Returns(1);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            if (shouldSucceed)
            {
                act.Should().NotThrowAsync();
            }
            else
            {
                act.Should().ThrowAsync<UnauthorizedAccessException>()
                    .WithMessage("User cannot delete other user's comments");
            }
        }

        [Test]
        public async Task Handle_WhenOwnerWithAnyRole_ShouldAllowDelete()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingComment = new Comment
            {
                CommentId = Guid.NewGuid(),
                TestOrderId = Guid.NewGuid(),
                CreateById = userId,
                CreateByName = "Dr. Smith",
                Content = "My comment",
                CreateAt = DateTime.UtcNow.AddDays(-1)
            };

            var command = new DeleteCommentCommand
            {
                TestOrderId = existingComment.TestOrderId,
                CommentId = existingComment.CommentId,
                UserId = userId, // Same user
                Role = "any_role" // Any role should work for owner
            };

            _commentRepository
                .GetCommentByIdAndTestOrderIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(existingComment);

            _commentRepository
                .DeleteCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            _unitOfWork
                .SaveChangesAsync(Arg.Any<CancellationToken>())
                .Returns(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task Handle_ShouldPerformHardDelete()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingComment = new Comment
            {
                CommentId = Guid.NewGuid(),
                TestOrderId = Guid.NewGuid(),
                CreateById = userId,
                CreateByName = "Dr. Smith",
                Content = "Comment to delete",
                CreateAt = DateTime.UtcNow.AddDays(-1)
            };

            var command = new DeleteCommentCommand
            {
                TestOrderId = existingComment.TestOrderId,
                CommentId = existingComment.CommentId,
                UserId = userId,
                Role = "doctor"
            };

            _commentRepository
                .GetCommentByIdAndTestOrderIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(existingComment);

            _commentRepository
                .DeleteCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            _unitOfWork
                .SaveChangesAsync(Arg.Any<CancellationToken>())
                .Returns(1);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            await _commentRepository
                .Received(1)
                .DeleteCommentAsync(
                    Arg.Is<Comment>(c => c.CommentId == existingComment.CommentId),
                    Arg.Any<CancellationToken>());
        }
    }
}
