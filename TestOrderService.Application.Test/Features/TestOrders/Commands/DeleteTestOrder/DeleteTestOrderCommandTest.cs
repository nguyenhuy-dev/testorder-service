using TestOrderService.Application.Features.TestOrders.Commands.DeleteTestOrder;
namespace TestOrderService.Application.Test.Features.TestOrders.Commands.DeleteTestOrder
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
