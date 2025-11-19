using TestOrderService.Application.Features.TestOrders.Commands;
namespace TestOrderService.Tests.Application.TestOrders.Commands
{
    [TestFixture]
    public class DeleteTestOrderCommandTest
    {
        [Test]
        public void Constructor_Should_Set_TestOrderId()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var command = new DeleteTestOrderCommand(id);

            // Assert
            if (command.TestOrderId != id)
                Assert.Fail($"Expected TestOrderId {id}, got {command.TestOrderId}");

            Assert.Pass();
        }
    }
}
