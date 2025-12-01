using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TestOrderService.Application.Exceptions;
using TestOrderService.Application.Features.TestOrders.Commands.UpdateTestOrder;
using TestOrderService.Application.Interfaces;
using TestOrderService.Application.Interfaces.EventBus;
using TestOrderService.Domain.Entities;
namespace TestOrderService.Application.Test.Features.TestOrders.Commands.UpdateTestOrder
{
    /// <summary>
    ///     UT for UpdateTestOrderCommandHandlerTests
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
        ///     The test order repository mock
        /// </summary>
        private Mock<ITestOrderRepository> _testOrderRepositoryMock = null!;
        /// <summary>
        ///     The unit of work mock
        /// </summary>
        private Mock<IUnitOfWork> _unitOfWorkMock = null!;
        /// <summary>
        ///     The event publisher mock
        /// </summary>
        private Mock<IEventPublisher> _eventPublisherMock = null!;
        /// <summary>
        ///     The logger mock
        /// </summary>
        private Mock<ILogger<UpdateTestOrderCommandHandler>> _loggerMock = null!;
        /// <summary>
        ///     The handler
        /// </summary>
        private UpdateTestOrderCommandHandler _handler = null!;

        // ---------------------------------------------
        // 1. Update success
        // ---------------------------------------------
        /// <summary>
        ///     Handles the updates test order successfully when test order exists.
        /// </summary>
        [Test]
        public async Task Handle_UpdatesTestOrderSuccessfully_WhenTestOrderExists()
        {
            var testOrderId = Guid.NewGuid();
            var updateById = Guid.NewGuid();
            var runById = Guid.NewGuid();
            var runAt = DateTime.UtcNow;

            var existingTestOrder = new TestOrder
            {
                TestOrderId = testOrderId,
                PatientId = Guid.NewGuid(),
                Status = StatusTestOrder.Pending,
                TestOrderDescription = "Old description",
                RunById = Guid.Empty,
                RunAt = null
            };

            var command = new UpdateTestOrderCommand
            {
                TestOrderId = testOrderId,
                RunById = runById,
                RunAt = runAt,
                Status = StatusTestOrder.Completed,
                UpdateById = updateById,
                UpdateAt = DateTime.UtcNow
            };

            _testOrderRepositoryMock
                .Setup(r => r.GetByIdAsync(testOrderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTestOrder);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.TestOrderId.Should().Be(testOrderId);
            result.RunById.Should().Be(runById);
            result.RunAt.Should().Be(runAt);
            result.Status.Should().Be(StatusTestOrder.Completed.ToString());

        }

        // ---------------------------------------------
        // 2. Not found
        // ---------------------------------------------
        /// <summary>
        ///     Handles the throws not found exception when test order does not exist.
        /// </summary>
        [Test]
        public async Task Handle_ThrowsNotFoundException_WhenTestOrderDoesNotExist()
        {
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

            var act = async () => await _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"TestOrderId: {testOrderId}");
        }

        // ---------------------------------------------
        // 3. Partial update
        // ---------------------------------------------
        /// <summary>
        ///     Handles the performs partial update when only some fields are provided.
        /// </summary>
        [Test]
        public async Task Handle_PerformsPartialUpdate_WhenOnlySomeFieldsAreProvided()
        {
            var testOrderId = Guid.NewGuid();
            var updateById = Guid.NewGuid();

            var existingTestOrder = new TestOrder
            {
                TestOrderId = testOrderId,
                Status = StatusTestOrder.Pending,
                RunById = Guid.NewGuid(),
                RunAt = DateTime.UtcNow
            };

            var command = new UpdateTestOrderCommand
            {
                TestOrderId = testOrderId,
                Status = StatusTestOrder.Completed,
                UpdateById = updateById,
                UpdateAt = DateTime.UtcNow
            };

            _testOrderRepositoryMock
                .Setup(r => r.GetByIdAsync(testOrderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTestOrder);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Status.Should().Be(StatusTestOrder.Completed.ToString());
        }

        // ---------------------------------------------
        // 4. Update RunById
        // ---------------------------------------------
        /// <summary>
        ///     Handles the updates run by identifier independently.
        /// </summary>
        [Test]
        public async Task Handle_UpdatesRunById_Independently()
        {
            var testOrderId = Guid.NewGuid();
            var newRunById = Guid.NewGuid();

            var existingTestOrder = new TestOrder
            {
                TestOrderId = testOrderId,
                RunById = Guid.Empty
            };

            var command = new UpdateTestOrderCommand
            {
                TestOrderId = testOrderId,
                RunById = newRunById,
                UpdateById = Guid.NewGuid(),
                UpdateAt = DateTime.UtcNow
            };

            _testOrderRepositoryMock.Setup(r => r.GetByIdAsync(testOrderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTestOrder);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.RunById.Should().Be(newRunById);
        }

        // ---------------------------------------------
        // 5. Update RunAt
        // ---------------------------------------------
        /// <summary>
        ///     Handles the updates run at independently.
        /// </summary>
        [Test]
        public async Task Handle_UpdatesRunAt_Independently()
        {
            var testOrderId = Guid.NewGuid();
            var newRunAt = DateTime.UtcNow;

            var existingTestOrder = new TestOrder { TestOrderId = testOrderId };

            var command = new UpdateTestOrderCommand
            {
                TestOrderId = testOrderId,
                RunAt = newRunAt,
                UpdateById = Guid.NewGuid(),
                UpdateAt = DateTime.UtcNow
            };

            _testOrderRepositoryMock
                .Setup(r => r.GetByIdAsync(testOrderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTestOrder);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.RunAt.Should().Be(newRunAt);
        }

        // ---------------------------------------------
        // 6. Preserve values when null
        // ---------------------------------------------
        /// <summary>
        ///     Handles the preserves existing values when null is provided for optional fields.
        /// </summary>
        [Test]
        public async Task Handle_PreservesExistingValues_WhenNullIsProvidedForOptionalFields()
        {
            var testOrderId = Guid.NewGuid();
            var existingRunById = Guid.NewGuid();
            var existingRunAt = DateTime.UtcNow.AddDays(-1);
            var existingStatus = StatusTestOrder.Pending;

            var existingTestOrder = new TestOrder
            {
                TestOrderId = testOrderId,
                RunById = existingRunById,
                RunAt = existingRunAt,
                Status = existingStatus,
                TestOrderDescription = "Desc"
            };

            var command = new UpdateTestOrderCommand
            {
                TestOrderId = testOrderId,
                RunById = null,
                RunAt = null,
                Status = null,
                TestOrderDescription = null,
                UpdateById = Guid.NewGuid(),
                UpdateAt = DateTime.UtcNow
            };

            _testOrderRepositoryMock
                .Setup(r => r.GetByIdAsync(testOrderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTestOrder);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.RunById.Should().Be(existingRunById);
            result.RunAt.Should().Be(existingRunAt);
            result.Status.Should().Be(existingStatus.ToString());
        }
    }
}
