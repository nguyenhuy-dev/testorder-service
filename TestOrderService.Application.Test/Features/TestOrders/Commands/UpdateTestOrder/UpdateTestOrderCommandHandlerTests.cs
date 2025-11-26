using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TestOrderService.Application.Exceptions;
using TestOrderService.Application.Features.TestOrders.Commands.UpdateTestOrder;
using TestOrderService.Application.IntegrationEvents;
using TestOrderService.Application.Interfaces;
using TestOrderService.Application.Interfaces.EventBus;
using TestOrderService.Domain.Entities;
namespace TestOrderService.Application.Test.Features.TestOrders.Commands.UpdateTestOrder
{
    /// <summary>
    ///     Unit test for UpdateTestOrderCommandHandler
    /// </summary>
    [TestFixture]
    public class UpdateTestOrderCommandHandlerTests
    {
        /// <summary>
        ///     Setups this instance.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _testOrderRepositoryMock = new Mock<ITestOrderRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _eventPublisherMock = new Mock<IEventPublisher>();
            _loggerMock = new Mock<ILogger<UpdateTestOrderCommandHandler>>();

            _handler = new UpdateTestOrderCommandHandler(
                _testOrderRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _eventPublisherMock.Object,
                _loggerMock.Object);
        }

        /// <summary>
        ///     The test order repository
        /// </summary>
        private Mock<ITestOrderRepository> _testOrderRepositoryMock = null!;

        /// <summary>
        ///     The unit of work
        /// </summary>
        private Mock<IUnitOfWork> _unitOfWorkMock = null!;

        /// <summary>
        ///     The event publisher
        /// </summary>
        private Mock<IEventPublisher> _eventPublisherMock = null!;

        /// <summary>
        ///     The logger
        /// </summary>
        private Mock<ILogger<UpdateTestOrderCommandHandler>> _loggerMock = null!;

        /// <summary>
        ///     The handler
        /// </summary>
        private UpdateTestOrderCommandHandler _handler = null!;

        /// <summary>
        ///     Handle updates test order successfully when test order exists.
        /// </summary>
        [Test]
        public async Task Handle_UpdatesTestOrderSuccessfully_WhenTestOrderExists()
        {
            // Arrange
            var testOrderId = Guid.NewGuid();
            var updateById = Guid.NewGuid();
            var runById = Guid.NewGuid();
            var runAt = DateTime.UtcNow;
            var updateAt = DateTime.UtcNow;

            var existingTestOrder = new TestOrder
            {
                TestOrderId = testOrderId,
                PatientId = Guid.NewGuid(),
                Status = StatusTestOrder.Pending,
                TestOrderDescription = "Old description",
                RunById = Guid.Empty,
                RunAt = null,
                CreateById = Guid.NewGuid(),
                CreateAt = DateTime.UtcNow.AddDays(-1),
                UpdateById = null,
                UpdateAt = null
            };

            var command = new UpdateTestOrderCommand
            {
                TestOrderId = testOrderId,
                RunById = runById,
                RunAt = runAt,
                TestOrderDescription = "New description",
                Status = StatusTestOrder.Completed,
                UpdateById = updateById,
                UpdateAt = updateAt
            };

            _testOrderRepositoryMock
                .Setup(r => r.GetByIdAsync(testOrderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTestOrder);
            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            _eventPublisherMock
                .Setup(e => e.PublishAsync(It.IsAny<TestOrderUpdatedIntegrationEvent>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.TestOrderId.Should().Be(testOrderId);
            result.RunById.Should().Be(runById);
            result.RunAt.Should().Be(runAt);
            result.TestOrderDescription.Should().Be("New description");
            result.Status.Should().Be(StatusTestOrder.Completed);
            result.UpdateById.Should().Be(updateById);
            result.UpdateAt.Should().Be(updateAt);

            _testOrderRepositoryMock.Verify(r => r.GetByIdAsync(testOrderId, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _eventPublisherMock.Verify(e => e.PublishAsync(It.IsAny<TestOrderUpdatedIntegrationEvent>()), Times.Once);
        }

        /// <summary>
        ///     Handle throws not found exception when test order does not exist.
        /// </summary>
        [Test]
        public async Task Handle_ThrowsNotFoundException_WhenTestOrderDoesNotExist()
        {
            // Arrange
            var testOrderId = Guid.NewGuid();
            var command = new UpdateTestOrderCommand
            {
                TestOrderId = testOrderId,
                UpdateById = Guid.NewGuid(),
                UpdateAt = DateTime.UtcNow
            };

            _testOrderRepositoryMock
                .Setup(r => r.GetByIdAsync(testOrderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TestOrder?)null);

            // Act
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"TestOrderId: {testOrderId}");

            _testOrderRepositoryMock.Verify(r => r.GetByIdAsync(testOrderId, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            _eventPublisherMock.Verify(e => e.PublishAsync(It.IsAny<TestOrderUpdatedIntegrationEvent>()), Times.Never);
        }

        /// <summary>
        ///     Handle performs partial update when only some fields are provided.
        /// </summary>
        [Test]
        public async Task Handle_PerformsPartialUpdate_WhenOnlySomeFieldsAreProvided()
        {
            // Arrange
            var testOrderId = Guid.NewGuid();
            var updateById = Guid.NewGuid();
            var originalDescription = "Original description";
            var originalStatus = StatusTestOrder.Pending;
            var originalRunById = Guid.NewGuid();
            var originalRunAt = DateTime.UtcNow.AddDays(-1);

            var existingTestOrder = new TestOrder
            {
                TestOrderId = testOrderId,
                PatientId = Guid.NewGuid(),
                Status = originalStatus,
                TestOrderDescription = originalDescription,
                RunById = originalRunById,
                RunAt = originalRunAt,
                CreateById = Guid.NewGuid(),
                CreateAt = DateTime.UtcNow.AddDays(-2),
                UpdateById = null,
                UpdateAt = null
            };

            var command = new UpdateTestOrderCommand
            {
                TestOrderId = testOrderId,
                Status = StatusTestOrder.Completed,
                UpdateById = updateById,
                UpdateAt = DateTime.UtcNow
                // RunById, RunAt, and TestOrderDescription are not provided
            };

            _testOrderRepositoryMock
                .Setup(r => r.GetByIdAsync(testOrderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTestOrder);
            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            _eventPublisherMock
                .Setup(e => e.PublishAsync(It.IsAny<TestOrderUpdatedIntegrationEvent>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(StatusTestOrder.Completed);
            result.TestOrderDescription.Should().Be(originalDescription); // Should remain unchanged
            result.RunById.Should().Be(originalRunById); // Should remain unchanged
            result.RunAt.Should().Be(originalRunAt); // Should remain unchanged
            result.UpdateById.Should().Be(updateById);
        }

        /// <summary>
        ///     Handle updates run by id independently.
        /// </summary>
        [Test]
        public async Task Handle_UpdatesRunById_Independently()
        {
            // Arrange
            var testOrderId = Guid.NewGuid();
            var updateById = Guid.NewGuid();
            var newRunById = Guid.NewGuid();

            var existingTestOrder = new TestOrder
            {
                TestOrderId = testOrderId,
                PatientId = Guid.NewGuid(),
                Status = StatusTestOrder.Pending,
                RunById = Guid.Empty,
                RunAt = null,
                CreateById = Guid.NewGuid(),
                CreateAt = DateTime.UtcNow,
                UpdateById = null,
                UpdateAt = null
            };

            var command = new UpdateTestOrderCommand
            {
                TestOrderId = testOrderId,
                RunById = newRunById,
                UpdateById = updateById,
                UpdateAt = DateTime.UtcNow
                // RunAt is not provided
            };

            _testOrderRepositoryMock
                .Setup(r => r.GetByIdAsync(testOrderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTestOrder);
            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            _eventPublisherMock
                .Setup(e => e.PublishAsync(It.IsAny<TestOrderUpdatedIntegrationEvent>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.RunById.Should().Be(newRunById);
            result.RunAt.Should().BeNull(); // Should remain null
        }

        /// <summary>
        ///     Handle updates run at independently.
        /// </summary>
        [Test]
        public async Task Handle_UpdatesRunAt_Independently()
        {
            // Arrange
            var testOrderId = Guid.NewGuid();
            var updateById = Guid.NewGuid();
            var newRunAt = DateTime.UtcNow;

            var existingTestOrder = new TestOrder
            {
                TestOrderId = testOrderId,
                PatientId = Guid.NewGuid(),
                Status = StatusTestOrder.Pending,
                RunById = Guid.Empty,
                RunAt = null,
                CreateById = Guid.NewGuid(),
                CreateAt = DateTime.UtcNow,
                UpdateById = null,
                UpdateAt = null
            };

            var command = new UpdateTestOrderCommand
            {
                TestOrderId = testOrderId,
                RunAt = newRunAt,
                UpdateById = updateById,
                UpdateAt = DateTime.UtcNow
                // RunById is not provided
            };

            _testOrderRepositoryMock
                .Setup(r => r.GetByIdAsync(testOrderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTestOrder);
            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            _eventPublisherMock
                .Setup(e => e.PublishAsync(It.IsAny<TestOrderUpdatedIntegrationEvent>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.RunAt.Should().Be(newRunAt);
            result.RunById.Should().Be(Guid.Empty); // Should remain unchanged
        }

        /// <summary>
        ///     Handle clears test order description when empty string is provided.
        /// </summary>
        [Test]
        public async Task Handle_ClearsTestOrderDescription_WhenEmptyStringIsProvided()
        {
            // Arrange
            var testOrderId = Guid.NewGuid();
            var updateById = Guid.NewGuid();

            var existingTestOrder = new TestOrder
            {
                TestOrderId = testOrderId,
                PatientId = Guid.NewGuid(),
                Status = StatusTestOrder.Pending,
                TestOrderDescription = "Some description",
                RunById = Guid.Empty,
                RunAt = null,
                CreateById = Guid.NewGuid(),
                CreateAt = DateTime.UtcNow,
                UpdateById = null,
                UpdateAt = null
            };

            var command = new UpdateTestOrderCommand
            {
                TestOrderId = testOrderId,
                TestOrderDescription = "", // Empty string
                UpdateById = updateById,
                UpdateAt = DateTime.UtcNow
            };

            _testOrderRepositoryMock
                .Setup(r => r.GetByIdAsync(testOrderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTestOrder);
            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            _eventPublisherMock
                .Setup(e => e.PublishAsync(It.IsAny<TestOrderUpdatedIntegrationEvent>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.TestOrderDescription.Should().BeNull(); // Empty string should become null
        }

        /// <summary>
        ///     Handle clears test order description when whitespace string is provided.
        /// </summary>
        [Test]
        public async Task Handle_ClearsTestOrderDescription_WhenWhitespaceStringIsProvided()
        {
            // Arrange
            var testOrderId = Guid.NewGuid();
            var updateById = Guid.NewGuid();

            var existingTestOrder = new TestOrder
            {
                TestOrderId = testOrderId,
                PatientId = Guid.NewGuid(),
                Status = StatusTestOrder.Pending,
                TestOrderDescription = "Some description",
                RunById = Guid.Empty,
                RunAt = null,
                CreateById = Guid.NewGuid(),
                CreateAt = DateTime.UtcNow,
                UpdateById = null,
                UpdateAt = null
            };

            var command = new UpdateTestOrderCommand
            {
                TestOrderId = testOrderId,
                TestOrderDescription = "   ", // Whitespace string
                UpdateById = updateById,
                UpdateAt = DateTime.UtcNow
            };

            _testOrderRepositoryMock
                .Setup(r => r.GetByIdAsync(testOrderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTestOrder);
            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            _eventPublisherMock
                .Setup(e => e.PublishAsync(It.IsAny<TestOrderUpdatedIntegrationEvent>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.TestOrderDescription.Should().BeNull(); // Whitespace should become null
        }

        /// <summary>
        ///     Handle always updates tracking fields.
        /// </summary>
        [Test]
        public async Task Handle_AlwaysUpdatesTrackingFields()
        {
            // Arrange
            var testOrderId = Guid.NewGuid();
            var updateById = Guid.NewGuid();
            var updateAt = DateTime.UtcNow;

            var existingTestOrder = new TestOrder
            {
                TestOrderId = testOrderId,
                PatientId = Guid.NewGuid(),
                Status = StatusTestOrder.Pending,
                CreateById = Guid.NewGuid(),
                CreateAt = DateTime.UtcNow.AddDays(-1),
                UpdateById = null,
                UpdateAt = null
            };

            var command = new UpdateTestOrderCommand
            {
                TestOrderId = testOrderId,
                UpdateById = updateById,
                UpdateAt = updateAt
                // No other fields provided
            };

            _testOrderRepositoryMock
                .Setup(r => r.GetByIdAsync(testOrderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTestOrder);
            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            _eventPublisherMock
                .Setup(e => e.PublishAsync(It.IsAny<TestOrderUpdatedIntegrationEvent>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.UpdateById.Should().Be(updateById);
            result.UpdateAt.Should().Be(updateAt);
        }

        /// <summary>
        ///     Handle updates all fields when all are provided.
        /// </summary>
        [Test]
        public async Task Handle_UpdatesAllFields_WhenAllAreProvided()
        {
            // Arrange
            var testOrderId = Guid.NewGuid();
            var updateById = Guid.NewGuid();
            var runById = Guid.NewGuid();
            var runAt = DateTime.UtcNow;
            var updateAt = DateTime.UtcNow;
            var newDescription = "Updated description";
            var newStatus = StatusTestOrder.Cancelled;

            var existingTestOrder = new TestOrder
            {
                TestOrderId = testOrderId,
                PatientId = Guid.NewGuid(),
                Status = StatusTestOrder.Pending,
                TestOrderDescription = "Old description",
                RunById = Guid.Empty,
                RunAt = null,
                CreateById = Guid.NewGuid(),
                CreateAt = DateTime.UtcNow.AddDays(-1),
                UpdateById = null,
                UpdateAt = null
            };

            var command = new UpdateTestOrderCommand
            {
                TestOrderId = testOrderId,
                RunById = runById,
                RunAt = runAt,
                TestOrderDescription = newDescription,
                Status = newStatus,
                UpdateById = updateById,
                UpdateAt = updateAt
            };

            _testOrderRepositoryMock
                .Setup(r => r.GetByIdAsync(testOrderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTestOrder);
            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            _eventPublisherMock
                .Setup(e => e.PublishAsync(It.IsAny<TestOrderUpdatedIntegrationEvent>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.RunById.Should().Be(runById);
            result.RunAt.Should().Be(runAt);
            result.TestOrderDescription.Should().Be(newDescription);
            result.Status.Should().Be(newStatus);
            result.UpdateById.Should().Be(updateById);
            result.UpdateAt.Should().Be(updateAt);
        }

        /// <summary>
        ///     Handle preserves existing values when null is provided for optional fields.
        /// </summary>
        [Test]
        public async Task Handle_PreservesExistingValues_WhenNullIsProvidedForOptionalFields()
        {
            // Arrange
            var testOrderId = Guid.NewGuid();
            var updateById = Guid.NewGuid();
            var existingRunById = Guid.NewGuid();
            var existingRunAt = DateTime.UtcNow.AddDays(-1);
            var existingDescription = "Existing description";
            var existingStatus = StatusTestOrder.Pending;

            var existingTestOrder = new TestOrder
            {
                TestOrderId = testOrderId,
                PatientId = Guid.NewGuid(),
                Status = existingStatus,
                TestOrderDescription = existingDescription,
                RunById = existingRunById,
                RunAt = existingRunAt,
                CreateById = Guid.NewGuid(),
                CreateAt = DateTime.UtcNow.AddDays(-2),
                UpdateById = null,
                UpdateAt = null
            };

            var command = new UpdateTestOrderCommand
            {
                TestOrderId = testOrderId,
                RunById = null,
                RunAt = null,
                TestOrderDescription = null,
                Status = null,
                UpdateById = updateById,
                UpdateAt = DateTime.UtcNow
            };

            _testOrderRepositoryMock
                .Setup(r => r.GetByIdAsync(testOrderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTestOrder);
            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            _eventPublisherMock
                .Setup(e => e.PublishAsync(It.IsAny<TestOrderUpdatedIntegrationEvent>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.RunById.Should().Be(existingRunById); // Should remain unchanged
            result.RunAt.Should().Be(existingRunAt); // Should remain unchanged
            result.TestOrderDescription.Should().Be(existingDescription); // Should remain unchanged
            result.Status.Should().Be(existingStatus); // Should remain unchanged
            result.UpdateById.Should().Be(updateById); // Should be updated
        }
    }
}
