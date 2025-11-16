using TestOrderService.Application.DTOs;
using TestOrderService.Application.Features.TestOrders.Queries.GetTestOrders;
using TestOrderService.Domain.Entities;
namespace TestOrderService.Application.Test.Features.TestOrders.Queries.GetTestOrders
{
    /// <summary>
    ///     Unit tests for GetTestOrdersQuery and GetTestOrdersRequest
    /// </summary>
    [TestFixture]
    public class GetTestOrdersQueryTests
    {

        [SetUp]
        public void Setup()
        {
            _request = new GetTestOrdersRequest();
        }
        private GetTestOrdersRequest _request = null!;

        [Test]
        public void Constructor_DefaultValues_PageNumberIsOne()
        {
            // Act & Assert
            Assert.That(_request.PageNumber, Is.EqualTo(1));
        }

        [Test]
        public void Constructor_DefaultValues_PageSizeIsTen()
        {
            // Act & Assert
            Assert.That(_request.PageSize, Is.EqualTo(10));
        }

        [Test]
        public void Constructor_DefaultValues_StatusIsNull()
        {
            // Act & Assert
            Assert.That(_request.Status, Is.Null);
        }

        [Test]
        public void PageNumber_CanBeSetAndRetrieved()
        {
            // Arrange
            var expectedValue = 5;

            // Act
            _request.PageNumber = expectedValue;

            // Assert
            Assert.That(_request.PageNumber, Is.EqualTo(expectedValue));
        }

        [Test]
        public void PageSize_CanBeSetAndRetrieved()
        {
            // Arrange
            var expectedValue = 25;

            // Act
            _request.PageSize = expectedValue;

            // Assert
            Assert.That(_request.PageSize, Is.EqualTo(expectedValue));
        }

        [Test]
        public void Status_CanBeSetAndRetrieved_Pending()
        {
            // Arrange
            var expectedValue = StatusTestOrder.Pending;

            // Act
            _request.Status = expectedValue;

            // Assert
            Assert.That(_request.Status, Is.EqualTo(expectedValue));
        }

        [Test]
        public void Status_CanBeSetAndRetrieved_Completed()
        {
            // Arrange
            var expectedValue = StatusTestOrder.Completed;

            // Act
            _request.Status = expectedValue;

            // Assert
            Assert.That(_request.Status, Is.EqualTo(expectedValue));
        }

        [Test]
        public void Status_CanBeSetAndRetrieved_Rejected()
        {
            // Arrange
            var expectedValue = StatusTestOrder.Rejected;

            // Act
            _request.Status = expectedValue;

            // Assert
            Assert.That(_request.Status, Is.EqualTo(expectedValue));
        }

        [Test]
        public void GetTestOrdersQuery_CanBeCreatedWithRequest()
        {
            // Arrange
            var request = new GetTestOrdersRequest
            {
                PageNumber = 2,
                PageSize = 20,
                Status = StatusTestOrder.Completed
            };

            // Act
            var query = new GetTestOrdersQuery(request);

            // Assert
            Assert.That(query, Is.Not.Null);
            Assert.That(query.Request, Is.EqualTo(request));
        }

        [Test]
        public void PageNumber_WhenSetToZero_StaysZero()
        {
            // Arrange
            _request.PageNumber = 0;

            // Act & Assert
            Assert.That(_request.PageNumber, Is.EqualTo(0));
        }

        [Test]
        public void PageSize_WhenSetToZero_StaysZero()
        {
            // Arrange
            _request.PageSize = 0;

            // Act & Assert
            Assert.That(_request.PageSize, Is.EqualTo(0));
        }

        [Test]
        public void GetTestOrdersRequest_CanBeCreatedWithAllProperties()
        {
            // Arrange & Act
            var request = new GetTestOrdersRequest
            {
                PageNumber = 3,
                PageSize = 15,
                Status = StatusTestOrder.Pending
            };

            // Assert
            Assert.That(request.PageNumber, Is.EqualTo(3));
            Assert.That(request.PageSize, Is.EqualTo(15));
            Assert.That(request.Status, Is.EqualTo(StatusTestOrder.Pending));
        }

        [Test]
        public void GetTestOrdersQuery_PreservesRequestProperties()
        {
            // Arrange
            var request = new GetTestOrdersRequest
            {
                PageNumber = 4,
                PageSize = 50,
                Status = StatusTestOrder.Rejected
            };

            // Act
            var query = new GetTestOrdersQuery(request);

            // Assert
            Assert.That(query.Request.PageNumber, Is.EqualTo(4));
            Assert.That(query.Request.PageSize, Is.EqualTo(50));
            Assert.That(query.Request.Status, Is.EqualTo(StatusTestOrder.Rejected));
        }
    }
}
