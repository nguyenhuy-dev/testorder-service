using TestOrderService.Application.Features.TestOrders.Queries.GetDetail;
namespace TestOrderService.Application.Test.Features.TestOrders.Queries.GetDetail
{
    /// <summary>
    ///     Unit tests for GetTestOrderDetailQuery and GetTestOrderDetailRequest
    /// </summary>
    [TestFixture]
    public class GetTestOrderDetailQueryTests
    {
        /// <summary>
        ///     Constructors the should set test order identifier.
        /// </summary>
        [Test]
        public void Constructor_ShouldSetTestOrderId()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var query = new GetTestOrderDetailQuery(id);

            // Assert
            Assert.That(query.TestOrderId, Is.EqualTo(id));
        }
    }
}
