using Microsoft.Extensions.Logging;
using Moq;
using TestOrderService.Application.Exceptions;
using TestOrderService.Application.Features.TestOrders.Commands;
using TestOrderService.Application.IntegrationEvents;
using TestOrderService.Application.Interfaces;
using TestOrderService.Application.Interfaces.EventBus;
using Entities=TestOrderService.Domain.Entities;

namespace TestOrderService.Tests.Application.TestOrders.Commands
{
    [TestFixture]
    public class DeleteTestOrderCommandHandlerTests
    {

        [SetUp]
        public void Setup()
        {
            _repositoryMock = new Mock<ITestOrderRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _eventPublisherMock = new Mock<IEventPublisher>();
            _loggerMock = new Mock<ILogger<DeleteTestOrderCommandHandler>>();

            _handler = new DeleteTestOrderCommandHandler(
                _repositoryMock.Object,
                _unitOfWorkMock.Object,
                _loggerMock.Object,
                _eventPublisherMock.Object);
        }
        private Mock<ITestOrderRepository> _repositoryMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IEventPublisher> _eventPublisherMock;
        private Mock<ILogger<DeleteTestOrderCommandHandler>> _loggerMock;

        private DeleteTestOrderCommandHandler _handler;

        [Test]
        public async Task Handle_Should_Delete_When_TestOrder_Completed()
        {
            // Arrange
            var id = Guid.NewGuid();
            var command = new DeleteTestOrderCommand(id);

            var entity = new Entities.TestOrder
            {
                TestOrderId = id,
                Status = Entities.StatusTestOrder.Completed
            };

            _repositoryMock
                .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            _eventPublisherMock
                .Setup(x => x.PublishAsync(It.IsAny<TestOrderDeletedIntegrationEvent>()))
                .Returns(Task.FromResult(true));

            _unitOfWorkMock
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert result true
            if (!result)
                Assert.Fail("Expected result = true when deletion success.");

            // Verify delete
            _repositoryMock.Verify(
                x => x.Delete(It.Is<Entities.TestOrder>(t => t.TestOrderId == id)),
                Times.Once);

            // Verify event
            _eventPublisherMock.Verify(
                x => x.PublishAsync(It.IsAny<TestOrderDeletedIntegrationEvent>()),
                Times.Once);

            // Verify save
            _unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);

            Assert.Pass();
        }

        [Test]
        public void Handle_Should_Throw_NotFound_When_TestOrder_NotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            var command = new DeleteTestOrderCommand(id);

            _repositoryMock
                .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Entities.TestOrder?)null);

            // Act
            var ex = Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(command, CancellationToken.None));

            // Assert
            if (ex == null)
                Assert.Fail("Expected NotFoundException.");

            Assert.Pass();
        }

        [Test]
        public void Handle_Should_Throw_BusinessRule_When_Not_Completed()
        {
            // Arrange
            var id = Guid.NewGuid();
            var command = new DeleteTestOrderCommand(id);

            var entity = new Entities.TestOrder
            {
                TestOrderId = id,
                Status = Entities.StatusTestOrder.Pending
            };

            _repositoryMock
                .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            // Act
            var ex = Assert.ThrowsAsync<BusinessRuleException>(() =>
                _handler.Handle(command, CancellationToken.None));

            // Assert
            if (ex == null)
                Assert.Fail("Expected BusinessRuleException.");

            Assert.Pass();
        }
    }
}
