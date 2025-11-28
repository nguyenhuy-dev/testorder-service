using FluentAssertions;
using NSubstitute;
using TestOrderService.Application.Exceptions;
using TestOrderService.Application.Features.MedicalTestResults.Commands.AddTestResultComment;
using TestOrderService.Application.Interfaces;
using TestOrderService.Application.Interfaces.gRPC;
using TestOrderService.Domain.Entities;
namespace TestOrderService.Application.Test.Features.MedicalTestResults.Commands.AddTestResultComment
{
    [TestFixture]
    public class AddTestResultCommentCommandHandlerTests
    {
        [SetUp]
        public void SetUp()
        {
            _testResultCommentRepository = Substitute.For<ITestResultCommentRepository>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _userGrpcClient = Substitute.For<IUserGrpcClient>();

            _handler = new AddTestResultCommentCommandHandler(
                _testResultCommentRepository,
                _unitOfWork,
                _userGrpcClient);
        }

        private ITestResultCommentRepository _testResultCommentRepository;
        private IUnitOfWork _unitOfWork;
        private IUserGrpcClient _userGrpcClient;
        private AddTestResultCommentCommandHandler _handler;

        [Test]
        public async Task Handle_WithValidRequest_ShouldCreateCommentSuccessfully()
        {
            // Arrange
            var testResultId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var userName = "Dr. John Smith";
            var content = "Patient shows positive response to treatment.";

            var command = new AddTestResultCommentCommand
            {
                TestResultId = testResultId,
                UserId = userId,
                Name = userName,
                Content = content
            };

            // Mock test result exists
            _testResultCommentRepository
                .TestResultExistsAsync(testResultId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(true));

            // Mock comment creation
            _testResultCommentRepository
                .CreateTestResultCommentAsync(Arg.Any<TestResultComment>(), Arg.Any<CancellationToken>())
                .Returns(callInfo =>
                {
                    var comment = callInfo.ArgAt<TestResultComment>(0);
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
            result.TestResultId.Should().Be(testResultId);
            result.TestResultCommentId.Should().NotBeEmpty();
            result.CreateAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            // Verify interactions
            await _testResultCommentRepository
                .Received(1)
                .TestResultExistsAsync(testResultId, Arg.Any<CancellationToken>());

            await _testResultCommentRepository
                .Received(1)
                .CreateTestResultCommentAsync(
                    Arg.Is<TestResultComment>(c =>
                        c.Content == content &&
                        c.CreateById == userId &&
                        c.CreateByName == userName &&
                        c.TestResultId == testResultId),
                    Arg.Any<CancellationToken>());

            await _unitOfWork
                .Received(1)
                .SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_WithValidRequest_ShouldSetCreateAtToCurrentUtcTime()
        {
            // Arrange
            var command = new AddTestResultCommentCommand
            {
                TestResultId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Dr. Jane Doe",
                Content = "Follow-up required."
            };

            _testResultCommentRepository
                .TestResultExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(true);

            _testResultCommentRepository
                .CreateTestResultCommentAsync(Arg.Any<TestResultComment>(), Arg.Any<CancellationToken>())
                .Returns(callInfo => Task.FromResult(callInfo.ArgAt<TestResultComment>(0)));

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
        public void Handle_WhenTestResultDoesNotExist_ShouldThrowNotFoundException()
        {
            // Arrange
            var testResultId = Guid.NewGuid();
            var command = new AddTestResultCommentCommand
            {
                TestResultId = testResultId,
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = "Test content"
            };

            _testResultCommentRepository
                .TestResultExistsAsync(testResultId, Arg.Any<CancellationToken>())
                .Returns(false);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"*{nameof(TestResult)}*{testResultId}*");

            // Verify CreateCommentAsync was never called
            _testResultCommentRepository
                .DidNotReceive()
                .CreateTestResultCommentAsync(Arg.Any<TestResultComment>(), Arg.Any<CancellationToken>());

            _unitOfWork
                .DidNotReceive()
                .SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public void Handle_WhenRepositoryCreateFails_ShouldPropagateException()
        {
            // Arrange
            var command = new AddTestResultCommentCommand
            {
                TestResultId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = "Test content"
            };

            _testResultCommentRepository
                .TestResultExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(true);

            _testResultCommentRepository
                .CreateTestResultCommentAsync(Arg.Any<TestResultComment>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromException<TestResultComment>(new InvalidOperationException("Insert failed")));

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
    }
}
