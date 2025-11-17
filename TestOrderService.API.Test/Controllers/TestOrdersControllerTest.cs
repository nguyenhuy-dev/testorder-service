using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TestOrderService.API.Commons;
using TestOrderService.API.Controllers;
using TestOrderService.Application.DTOs;
using TestOrderService.Application.Exceptions;
using TestOrderService.Application.Features.TestOrders.Queries.GetDetail;
using TestOrderService.Application.Features.TestOrders.Queries.GetTestOrders;
using TestOrderService.Domain.Entities;
namespace TestOrderService.API.Test.Controllers
{
    [TestFixture]
    public class TestOrdersControllerTests
    {

        [SetUp]
        public void Setup()
        {
            _mockSender = new Mock<ISender>();
            _controller = new TestOrdersController(_mockSender.Object);
        }
        private Mock<ISender> _mockSender = null!;
        private TestOrdersController _controller = null!;

        private static GetTestOrdersRequest CreateValidGetTestOrdersRequest()
        {
            return new GetTestOrdersRequest
            {
                PageNumber = 1,
                PageSize = 10,
                Status = StatusTestOrder.Pending
            };
        }

        private static PaginatedList<TestOrderDto> CreateValidPaginatedTestOrders()
        {
            return new PaginatedList<TestOrderDto>(
                new List<TestOrderDto>
                {
                    new TestOrderDto
                    {
                        TestOrderId = Guid.NewGuid(),
                        PatientId = Guid.NewGuid(),
                        FullName = "Nguyen Van A",
                        Age = 25,
                        Phone = "0912345678",
                        Gender = true,
                        Status = "Pending",
                        CreateAt = DateTime.UtcNow,
                        CreateById = Guid.NewGuid(),
                        CreateByName = "Dr. John",
                        RunAt = null,
                        RunById = Guid.Empty,
                        RunByName = null
                    }
                },
                1,
                1,
                10
            );
        }

        [Test]
        public void Constructor_ShouldInitializeDependencies()
        {
            // Assert
            Assert.That(_controller, Is.Not.Null);
        }

        [Test]
        public async Task GetTestOrders_ShouldReturnOk_WhenQuerySuccessful()
        {
            // Arrange
            var request = CreateValidGetTestOrdersRequest();
            var result = CreateValidPaginatedTestOrders();

            _mockSender.Setup(s => s.Send(It.IsAny<GetTestOrdersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.GetTestOrders(request, CancellationToken.None);

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.StatusCode, Is.EqualTo(StatusCodes.Status200OK));

            var apiResponse = okResult.Value as ApiResponse<PaginatedList<TestOrderDto>>;
            Assert.Multiple(() =>
            {
                Assert.That(apiResponse, Is.Not.Null);
                Assert.That(apiResponse!.StatusCode, Is.EqualTo(200));
                Assert.That(apiResponse.Message, Is.EqualTo("Request successful."));
                Assert.That(apiResponse.Data, Is.EqualTo(result));
            });

            _mockSender.Verify(s => s.Send(It.IsAny<GetTestOrdersQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void GetTestOrders_ShouldThrowException_WhenSenderFails()
        {
            // Arrange
            var request = CreateValidGetTestOrdersRequest();
            var expectedException = new Exception("Database connection failed");

            _mockSender.Setup(s => s.Send(It.IsAny<GetTestOrdersQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            // Act + Assert
            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await _controller.GetTestOrders(request, CancellationToken.None)
            );

            Assert.That(ex, Is.EqualTo(expectedException));
        }

        [Test]
        public async Task GetTestOrders_ShouldHandleDefaultQueryParameters()
        {
            // Arrange
            var request = new GetTestOrdersRequest(); // Default values
            var result = CreateValidPaginatedTestOrders();

            _mockSender.Setup(s => s.Send(It.IsAny<GetTestOrdersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.GetTestOrders(request, CancellationToken.None);

            // Assert
            var okResult = (OkObjectResult)actionResult;
            var apiResponse = (ApiResponse<PaginatedList<TestOrderDto>>)okResult.Value!;
            Assert.That(apiResponse.Data, Is.Not.Null);
        }

        [Test]
        public async Task GetTestOrders_ShouldReturnEmptyList_WhenNoOrdersExist()
        {
            // Arrange
            var request = CreateValidGetTestOrdersRequest();
            var emptyResult = new PaginatedList<TestOrderDto>(
                new List<TestOrderDto>(),
                0,
                1,
                10
            );

            _mockSender.Setup(s => s.Send(It.IsAny<GetTestOrdersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyResult);

            // Act
            var actionResult = await _controller.GetTestOrders(request, CancellationToken.None);

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var apiResponse = okResult!.Value as ApiResponse<PaginatedList<TestOrderDto>>;
            Assert.Multiple(() =>
            {
                Assert.That(apiResponse, Is.Not.Null);
                Assert.That(apiResponse!.Data.Items, Is.Empty);
                Assert.That(apiResponse.Data.TotalCount, Is.EqualTo(0));
            });
        }

        [Test]
        public async Task GetTestOrders_ShouldFilterByStatus_WhenStatusProvided()
        {
            // Arrange
            var request = new GetTestOrdersRequest
            {
                PageNumber = 1,
                PageSize = 10,
                Status = StatusTestOrder.Completed
            };

            var result = new PaginatedList<TestOrderDto>(
                new List<TestOrderDto>
                {
                    new TestOrderDto
                    {
                        TestOrderId = Guid.NewGuid(),
                        PatientId = Guid.NewGuid(),
                        FullName = "Test Patient",
                        Age = 30,
                        Phone = "0123456789",
                        Gender = false,
                        Status = "Completed",
                        CreateAt = DateTime.UtcNow,
                        CreateById = Guid.NewGuid(),
                        CreateByName = "Dr. Smith",
                        RunAt = DateTime.UtcNow,
                        RunById = Guid.NewGuid(),
                        RunByName = "Lab Tech"
                    }
                },
                1,
                1,
                10
            );

            _mockSender.Setup(s => s.Send(It.IsAny<GetTestOrdersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.GetTestOrders(request, CancellationToken.None);

            // Assert
            var okResult = actionResult as OkObjectResult;
            var apiResponse = okResult!.Value as ApiResponse<PaginatedList<TestOrderDto>>;

            Assert.That(apiResponse!.Data.Items.All(x => x.Status == "Completed"), Is.True);
        }

        [Test]
        public async Task GetTestOrders_ShouldReturnCorrectPaginationInfo()
        {
            // Arrange
            var request = new GetTestOrdersRequest
            {
                PageNumber = 2,
                PageSize = 5,
                Status = null
            };

            var items = new List<TestOrderDto>();
            for (var i = 0; i < 5; i++)
            {
                items.Add(new TestOrderDto
                {
                    TestOrderId = Guid.NewGuid(),
                    PatientId = Guid.NewGuid(),
                    FullName = $"Patient {i}",
                    Age = 20 + i,
                    Phone = "0900000000",
                    Gender = i % 2 == 0,
                    Status = "Pending",
                    CreateAt = DateTime.UtcNow,
                    CreateById = Guid.NewGuid(),
                    CreateByName = "Doctor",
                    RunAt = null,
                    RunById = Guid.Empty,
                    RunByName = null
                });
            }

            var result = new PaginatedList<TestOrderDto>(items, 15, 2, 5);

            _mockSender.Setup(s => s.Send(It.IsAny<GetTestOrdersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.GetTestOrders(request, CancellationToken.None);

            // Assert
            var okResult = actionResult as OkObjectResult;
            var apiResponse = okResult!.Value as ApiResponse<PaginatedList<TestOrderDto>>;

            Assert.Multiple(() =>
            {
                Assert.That(apiResponse!.Data.Items.Count, Is.EqualTo(5));
                Assert.That(apiResponse.Data.TotalCount, Is.EqualTo(15));
                Assert.That(apiResponse.Data.PageNumber, Is.EqualTo(2));
                Assert.That(apiResponse.Data.PageSize, Is.EqualTo(5));
                Assert.That(apiResponse.Data.TotalPages, Is.EqualTo(3));
            });
        }

        [Test]
        public async Task GetTestOrders_ShouldCreateCorrectQuery()
        {
            // Arrange
            var request = new GetTestOrdersRequest
            {
                PageNumber = 3,
                PageSize = 15,
                Status = StatusTestOrder.Rejected
            };

            var result = CreateValidPaginatedTestOrders();

            GetTestOrdersQuery? capturedQuery = null;
            _mockSender.Setup(s => s.Send(It.IsAny<GetTestOrdersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result)
                .Callback<IRequest<PaginatedList<TestOrderDto>>, CancellationToken>((query, _) =>
                {
                    capturedQuery = query as GetTestOrdersQuery;
                });

            // Act
            await _controller.GetTestOrders(request, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(capturedQuery, Is.Not.Null);
                Assert.That(capturedQuery!.Request.PageNumber, Is.EqualTo(3));
                Assert.That(capturedQuery.Request.PageSize, Is.EqualTo(15));
                Assert.That(capturedQuery.Request.Status, Is.EqualTo(StatusTestOrder.Rejected));
            });

            _mockSender.Verify(s => s.Send(It.IsAny<GetTestOrdersQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetTestOrders_ShouldHandleNullStatus()
        {
            // Arrange
            var request = new GetTestOrdersRequest
            {
                PageNumber = 1,
                PageSize = 10,
                Status = null
            };

            var result = CreateValidPaginatedTestOrders();

            _mockSender.Setup(s => s.Send(It.IsAny<GetTestOrdersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.GetTestOrders(request, CancellationToken.None);

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }

        [Test]
        public async Task GetTestOrders_ShouldIncludeUserNames_InResponse()
        {
            // Arrange
            var request = CreateValidGetTestOrdersRequest();
            var testOrders = new List<TestOrderDto>
            {
                new TestOrderDto
                {
                    TestOrderId = Guid.NewGuid(),
                    PatientId = Guid.NewGuid(),
                    FullName = "Patient Name",
                    Age = 35,
                    Phone = "0987654321",
                    Gender = true,
                    Status = "Completed",
                    CreateAt = DateTime.UtcNow.AddDays(-2),
                    CreateById = Guid.NewGuid(),
                    CreateByName = "Dr. Alice",
                    RunAt = DateTime.UtcNow.AddDays(-1),
                    RunById = Guid.NewGuid(),
                    RunByName = "Dr. Bob"
                }
            };

            var result = new PaginatedList<TestOrderDto>(testOrders, 1, 1, 10);

            _mockSender.Setup(s => s.Send(It.IsAny<GetTestOrdersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.GetTestOrders(request, CancellationToken.None);

            // Assert
            var okResult = actionResult as OkObjectResult;
            var apiResponse = okResult!.Value as ApiResponse<PaginatedList<TestOrderDto>>;
            var firstOrder = apiResponse!.Data.Items.First();

            Assert.Multiple(() =>
            {
                Assert.That(firstOrder.CreateByName, Is.EqualTo("Dr. Alice"));
                Assert.That(firstOrder.RunByName, Is.EqualTo("Dr. Bob"));
            });
        }

        [Test]
        public async Task GetTestOrderDetail_ShouldReturnOk_WhenQuerySuccessful()
        {
            // Arrange
            var id = Guid.NewGuid();
            var detail = new TestOrderDetailDto
            {
                TestOrderId = id,
                PatientId = Guid.NewGuid(),
                PatientName = "Test Patient",
                Status = "Pending",
                CreatedBy = "Doctor A"
            };

            _mockSender.Setup(s =>
                    s.Send(It.IsAny<GetTestOrderDetailQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(detail);

            // Act
            var actionResult = await _controller.GetTestOrderDetail(id, CancellationToken.None);

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.StatusCode, Is.EqualTo(200));

            var apiResponse = okResult.Value as ApiResponse<TestOrderDetailDto>;
            Assert.That(apiResponse!.Data.TestOrderId, Is.EqualTo(id));
        }



        [Test]
        public void GetTestOrderDetail_ShouldThrowNotFound_WhenHandlerThrowsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            var expected = new NotFoundException("TestOrder not found");

            _mockSender
                .Setup(s => s.Send(It.IsAny<GetTestOrderDetailQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expected);

            // Act
            var ex = Assert.ThrowsAsync<NotFoundException>(() => _controller.GetTestOrderDetail(id, CancellationToken.None));

            // Assert
            Assert.That(ex!.Message, Is.EqualTo("TestOrder not found"));
        }



        [Test]
        public void GetTestOrderDetail_ShouldThrowForbidden_WhenHandlerThrowsForbidden()
        {
            // Arrange
            var id = Guid.NewGuid();
            var expected = new ForbiddenAccessException("Forbidden access");

            _mockSender
                .Setup(s => s.Send(It.IsAny<GetTestOrderDetailQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expected);

            // Act
            var ex = Assert.ThrowsAsync<ForbiddenAccessException>(() => _controller.GetTestOrderDetail(id, CancellationToken.None));

            // Assert
            Assert.That(ex!.Message, Is.EqualTo("Forbidden access"));
        }



        [Test]
        public void GetTestOrderDetail_ShouldThrowInternalServerError_WhenUnexpectedExceptionOccurs()
        {
            // Arrange
            var id = Guid.NewGuid();
            var expected = new Exception("Unexpected error");

            _mockSender
                .Setup(s => s.Send(It.IsAny<GetTestOrderDetailQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expected);

            // Act
            var ex = Assert.ThrowsAsync<Exception>(() => _controller.GetTestOrderDetail(id, CancellationToken.None));

            // Assert
            Assert.That(ex!.Message, Is.EqualTo("Unexpected error"));
        }
    }
}
