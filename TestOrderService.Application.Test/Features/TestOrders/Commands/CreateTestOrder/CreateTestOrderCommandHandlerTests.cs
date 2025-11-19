using Microsoft.Extensions.Logging;
using Moq;
using TestOrderService.Application.Features.TestOrders.Commands.CreateTestOrder;
using TestOrderService.Application.IntegrationEvents;
using TestOrderService.Application.Interfaces;
using TestOrderService.Application.Interfaces.EventBus;
using Entities=TestOrderService.Domain.Entities;

namespace TestOrderService.Application.Test.Features.TestOrders.Commands.CreateTestOrder
{
    [TestFixture]
    public class CreateTestOrderCommandHandlerTests
    {

        [SetUp]
        public void Setup()
        {
            _repositoryMock = new Mock<ITestOrderRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _eventPublisherMock = new Mock<IEventPublisher>();
            _loggerMock = new Mock<ILogger<CreateTestOrderCommandHandler>>();

            _handler = new CreateTestOrderCommandHandler(
                _repositoryMock.Object,
                _unitOfWorkMock.Object,
                _eventPublisherMock.Object,
                _loggerMock.Object);
        }
        private Mock<ITestOrderRepository> _repositoryMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IEventPublisher> _eventPublisherMock;
        private Mock<ILogger<CreateTestOrderCommandHandler>> _loggerMock;
        private CreateTestOrderCommandHandler _handler;

        [Test]
        public async Task Handle_Should_Create_TestOrder_And_Publish_Event_And_SaveChanges()
        {
            // Arrange valid command
            var command = new CreateTestOrderCommand
            {
                PatientId = Guid.NewGuid(),
                TestOrderDescription = "Urgent blood test",
                CreateById = Guid.NewGuid()
            };

            // Simulate repository creation
            var createdEntity = new Entities.TestOrder
            {
                TestOrderId = command.TestOrderId,
                PatientId = command.PatientId,
                TestOrderDescription = command.TestOrderDescription,
                Status = command.Status,
                CreateAt = command.CreateAt,
                CreateById = command.CreateById
            };

            _repositoryMock
                .Setup(x => x.CreateTestOrderAsync(It.IsAny<Entities.TestOrder>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdEntity);

            _eventPublisherMock
                .Setup(x => x.PublishAsync(It.IsAny<TestOrderCreatedIntegrationEvent>()))
                .Returns(Task.FromResult(true));

            _unitOfWorkMock
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(1));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert not null
            if (result == null)
                Assert.Fail("Result should not be null");

            // Assert properties
            if (result!.TestOrderId != createdEntity.TestOrderId)
                Assert.Fail($"Expected TestOrderId {createdEntity.TestOrderId}, got {result.TestOrderId}");

            if (result.PatientId != createdEntity.PatientId)
                Assert.Fail($"Expected PatientId {createdEntity.PatientId}, got {result.PatientId}");

            if (result.TestOrderDescription != createdEntity.TestOrderDescription)
                Assert.Fail($"Expected TestOrderDescription {createdEntity.TestOrderDescription}, got {result.TestOrderDescription}");

            if (result.Status != createdEntity.Status)
                Assert.Fail($"Expected Status {createdEntity.Status}, got {result.Status}");

            if (result.CreateAt != createdEntity.CreateAt)
                Assert.Fail($"Expected CreateAt {createdEntity.CreateAt}, got {result.CreateAt}");

            if (result.CreateById != createdEntity.CreateById)
                Assert.Fail($"Expected CreateById {createdEntity.CreateById}, got {result.CreateById}");

            if (result.RunAt != createdEntity.RunAt)
                Assert.Fail($"Expected RunAt {createdEntity.RunAt}, got {result.RunAt}");

            if (result.RunById != createdEntity.RunById)
                Assert.Fail($"Expected RunById {createdEntity.RunById}, got {result.RunById}");

            // Verify repository call
            _repositoryMock.Verify(
                x => x.CreateTestOrderAsync(It.IsAny<Entities.TestOrder>(), It.IsAny<CancellationToken>()),
                Times.Once);

            // Verify event published
            _eventPublisherMock.Verify(
                x => x.PublishAsync(It.IsAny<TestOrderCreatedIntegrationEvent>()),
                Times.Once);

            // Verify unit of work saved
            _unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);

            Assert.Pass(); // All checks passed
        }

        [Test]
        public async Task Handle_Should_Handle_Empty_Description()
        {
            // Arrange command with empty description
            var command = new CreateTestOrderCommand
            {
                PatientId = Guid.NewGuid(),
                TestOrderDescription = "",
                CreateById = Guid.NewGuid()
            };

            var createdEntity = new Entities.TestOrder
            {
                TestOrderId = command.TestOrderId,
                PatientId = command.PatientId,
                TestOrderDescription = command.TestOrderDescription
            };

            _repositoryMock.Setup(x => x.CreateTestOrderAsync(It.IsAny<Entities.TestOrder>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdEntity);

            _eventPublisherMock.Setup(x => x.PublishAsync(It.IsAny<TestOrderCreatedIntegrationEvent>()))
                .Returns(Task.FromResult(true));

            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(1));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            if (result == null)
                Assert.Fail("Result should not be null");

            if (result!.TestOrderDescription != "")
                Assert.Fail($"Expected empty description, got {result.TestOrderDescription}");

            Assert.Pass();
        }
    }
}
