using FluentAssertions;
using NSubstitute;
using TestOrderService.Application.Exceptions;
using TestOrderService.Application.Features.TestOrders.Commands.AddComment;
using TestOrderService.Application.Interfaces;
using TestOrderService.Application.Interfaces.gRPC;
using TestOrderService.Domain.Entities;
namespace TestOrderService.Application.Test.Features.TestOrders.Commands.AddComment
{
    [TestFixture]
    public class AddCommentCommandHandlerTests
    {

        [SetUp]
        public void SetUp()
        {
            // Arrange - Create mocks
            _commentRepository = Substitute.For<ICommentRepository>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _userGrpcClient = Substitute.For<IUserGrpcClient>();

            // Create handler with mocked dependencies
            _handler = new AddCommentCommandHandler(
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
        private AddCommentCommandHandler _handler;

        [Test]
        public async Task Handle_WithValidRequest_ShouldCreateCommentSuccessfully()
        {
            // Arrange
            var testOrderId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var userName = "Dr. John Smith";
            var content = "Patient shows positive response to treatment.";

            var command = new AddCommentCommand
            {
                TestOrderId = testOrderId,
                UserId = userId,
                Name = userName,
                Content = content
            };

            // Mock test order exists
            _commentRepository
                .TestOrderExistsAsync(testOrderId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(true));

            // Mock comment creation
            _commentRepository
                .CreateCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>())
                .Returns(callInfo =>
                {
                    var comment = callInfo.ArgAt<Comment>(0);
                    return Task.FromResult(comment);
                });

            _unitOfWork
                .SaveChangesAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(1));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Content.Should().Be(content);
            result.CreateById.Should().Be(userId);
            result.CreateByName.Should().Be(userName);
            result.TestOrderId.Should().Be(testOrderId);
            result.CommentId.Should().NotBeEmpty();
            result.CreateAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            // Verify interactions
            await _commentRepository
                .Received(1)
                .TestOrderExistsAsync(testOrderId, Arg.Any<CancellationToken>());

            await _commentRepository
                .Received(1)
                .CreateCommentAsync(
                    Arg.Is<Comment>(c =>
                        c.Content == content &&
                        c.CreateById == userId &&
                        c.CreateByName == userName &&
                        c.TestOrderId == testOrderId),
                    Arg.Any<CancellationToken>());

            await _unitOfWork
                .Received(1)
                .SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_WithValidRequest_ShouldSetCreateAtToCurrentUtcTime()
        {
            // Arrange
            var command = new AddCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Dr. Jane Doe",
                Content = "Follow-up required."
            };

            _commentRepository
                .TestOrderExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(true);

            _commentRepository
                .CreateCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>())
                .Returns(callInfo => Task.FromResult(callInfo.ArgAt<Comment>(0)));

            _unitOfWork
                .SaveChangesAsync(Arg.Any<CancellationToken>())
                .Returns(1);

            var beforeExecution = DateTime.UtcNow;

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            var afterExecution = DateTime.UtcNow;

            // Assert
            result.CreateAt.Should().BeOnOrAfter(beforeExecution);
            result.CreateAt.Should().BeOnOrBefore(afterExecution);
        }

        [Test]
        public async Task Handle_WithValidRequest_ShouldGenerateUniqueCommentId()
        {
            // Arrange
            var command = new AddCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = "Test content"
            };

            _commentRepository
                .TestOrderExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(true);

            _commentRepository
                .CreateCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>())
                .Returns(callInfo => Task.FromResult(callInfo.ArgAt<Comment>(0)));

            _unitOfWork
                .SaveChangesAsync(Arg.Any<CancellationToken>())
                .Returns(1);

            // Act
            var result1 = await _handler.Handle(command, CancellationToken.None);
            var result2 = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result1.CommentId.Should().NotBeEmpty();
            result2.CommentId.Should().NotBeEmpty();
            result1.CommentId.Should().NotBe(result2.CommentId);
        }

        [Test]
        public async Task Handle_WithLongContent_ShouldAcceptContentUpTo2000Characters()
        {
            // Arrange
            var longContent = new string('A', 2000);
            var command = new AddCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = longContent
            };

            _commentRepository
                .TestOrderExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(true);

            _commentRepository
                .CreateCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>())
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
        public void Handle_WhenTestOrderDoesNotExist_ShouldThrowNotFoundException()
        {
            // Arrange
            var testOrderId = Guid.NewGuid();
            var command = new AddCommentCommand
            {
                TestOrderId = testOrderId,
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = "Test content"
            };

            _commentRepository
                .TestOrderExistsAsync(testOrderId, Arg.Any<CancellationToken>())
                .Returns(false);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"*{nameof(TestOrder)}*{testOrderId}*");

            // Verify CreateCommentAsync was never called
            _commentRepository
                .DidNotReceive()
                .CreateCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());

            _unitOfWork
                .DidNotReceive()
                .SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public void Handle_WhenTestOrderExistsCheckThrowsException_ShouldPropagateException()
        {
            // Arrange
            var command = new AddCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = "Test content"
            };

            _commentRepository
                .TestOrderExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromException<bool>(new InvalidOperationException("Database error")));

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Database error");
        }

        [Test]
        public void Handle_WhenRepositoryCreateFails_ShouldPropagateException()
        {
            // Arrange
            var command = new AddCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = "Test content"
            };

            _commentRepository
                .TestOrderExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(true);

            _commentRepository
                .CreateCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromException<Comment>(new InvalidOperationException("Insert failed")));

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Insert failed");

            // Verify SaveChanges was never called
            _unitOfWork
                .DidNotReceive()
                .SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public void Handle_WhenUnitOfWorkSaveFails_ShouldPropagateException()
        {
            // Arrange
            var command = new AddCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = "Test content"
            };

            _commentRepository
                .TestOrderExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(true);

            _commentRepository
                .CreateCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>())
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
        public async Task Handle_WithEmptyGuidTestOrderId_ShouldStillCheckIfExists()
        {
            // Arrange
            var emptyGuid = Guid.Empty;
            var command = new AddCommentCommand
            {
                TestOrderId = emptyGuid,
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = "Test content"
            };

            _commentRepository
                .TestOrderExistsAsync(emptyGuid, Arg.Any<CancellationToken>())
                .Returns(false);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();

            await _commentRepository
                .Received(1)
                .TestOrderExistsAsync(emptyGuid, Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_WithCancellationToken_ShouldPassTokenToRepositoryMethods()
        {
            // Arrange
            var command = new AddCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = "Test content"
            };

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            _commentRepository
                .TestOrderExistsAsync(Arg.Any<Guid>(), cancellationToken)
                .Returns(true);

            _commentRepository
                .CreateCommentAsync(Arg.Any<Comment>(), cancellationToken)
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
                .TestOrderExistsAsync(Arg.Any<Guid>(), cancellationToken);

            await _commentRepository
                .Received(1)
                .CreateCommentAsync(Arg.Any<Comment>(), cancellationToken);

            await _unitOfWork
                .Received(1)
                .SaveChangesAsync(cancellationToken);
        }

        [Test]
        public async Task Handle_WithMinimalContent_ShouldCreateCommentSuccessfully()
        {
            // Arrange
            var command = new AddCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = "A" // Single character
            };

            _commentRepository
                .TestOrderExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(true);

            _commentRepository
                .CreateCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>())
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
            var specialContent = "Test with special chars: @#$%^&*()_+-=[]{}|;':\"<>?,./";
            var command = new AddCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = specialContent
            };

            _commentRepository
                .TestOrderExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(true);

            _commentRepository
                .CreateCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>())
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
            var unicodeContent = "Test with unicode: 你好世界 مرحبا العالم שלום עולם";
            var command = new AddCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = unicodeContent
            };

            _commentRepository
                .TestOrderExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(true);

            _commentRepository
                .CreateCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>())
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
        public async Task Handle_ShouldCallRepositoryMethodsInCorrectOrder()
        {
            // Arrange
            var command = new AddCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = "Test content"
            };

            var callOrder = new List<string>();

            _commentRepository
                .TestOrderExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(callInfo =>
                {
                    callOrder.Add("TestOrderExistsAsync");
                    return Task.FromResult(true);
                });

            _commentRepository
                .CreateCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>())
                .Returns(callInfo =>
                {
                    callOrder.Add("CreateCommentAsync");
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
                "TestOrderExistsAsync",
                "CreateCommentAsync",
                "SaveChangesAsync"
            );
        }
    }
}
