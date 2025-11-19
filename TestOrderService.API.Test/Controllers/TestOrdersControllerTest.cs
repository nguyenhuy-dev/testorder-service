using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using TestOrderService.API.Commons;
using TestOrderService.API.Controllers;
using TestOrderService.API.Middleware;
using TestOrderService.Application.DTOs;
using TestOrderService.Application.Exceptions;
using TestOrderService.Application.Features.TestOrders.Commands.CreateTestOrder;
using TestOrderService.Application.Features.TestOrders.Commands.DeleteTestOrder;
using TestOrderService.Application.Features.TestOrders.Commands.UpdateTestOrder;
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
                Assert.That(apiResponse!.Data!.Items, Is.Empty);
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

            Assert.That(apiResponse!.Data!.Items.All(x => x.Status == "Completed"), Is.True);
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
                Assert.That(apiResponse!.Data!.Items.Count, Is.EqualTo(5));
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
            var firstOrder = apiResponse!.Data!.Items[0];

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
            Assert.That(apiResponse!.Data!.TestOrderId, Is.EqualTo(id));
        }

        [Test]
        public async Task UpdateTestOrder_ShouldReturnOk_WhenUpdateSuccessful()
        {
            // Arrange
            var testOrderId = Guid.NewGuid();
            var updateById = Guid.NewGuid();
            var updateDto = new UpdateTestOrderDto(
                Guid.NewGuid(),
                DateTime.UtcNow,
                "Updated description",
                StatusTestOrder.Completed
            );

            var updatedTestOrder = new TestOrder
            {
                TestOrderId = testOrderId,
                PatientId = Guid.NewGuid(),
                Status = StatusTestOrder.Completed,
                TestOrderDescription = "Updated description",
                RunById = updateDto.RunById!.Value,
                RunAt = updateDto.RunAt,
                CreateById = Guid.NewGuid(),
                CreateAt = DateTime.UtcNow.AddDays(-1),
                UpdateById = updateById,
                UpdateAt = DateTime.UtcNow
            };

            _mockSender.Setup(s => s.Send(It.IsAny<IRequest<TestOrder>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedTestOrder);

            // Setup controller context with user ID header
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _controller.ControllerContext.HttpContext.Request.Headers["X-User-Id"] = updateById.ToString();

            // Act
            var actionResult = await _controller.UpdateTestOrder(testOrderId, updateDto, CancellationToken.None);

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.StatusCode, Is.EqualTo(StatusCodes.Status200OK));

            var apiResponse = okResult.Value as ApiResponse<TestOrder>;
            Assert.Multiple(() =>
            {
                Assert.That(apiResponse, Is.Not.Null);
                Assert.That(apiResponse!.StatusCode, Is.EqualTo(200));
                Assert.That(apiResponse.Message, Is.EqualTo("Test order updated successfully."));
                Assert.That(apiResponse.Data, Is.EqualTo(updatedTestOrder));
            });

            _mockSender.Verify(s => s.Send(It.Is<IRequest<TestOrder>>(request =>
                request as UpdateTestOrderCommand != null &&
                (request as UpdateTestOrderCommand)!.TestOrderId == testOrderId &&
                (request as UpdateTestOrderCommand)!.UpdateById == updateById &&
                (request as UpdateTestOrderCommand)!.RunById == updateDto.RunById &&
                (request as UpdateTestOrderCommand)!.RunAt == updateDto.RunAt &&
                (request as UpdateTestOrderCommand)!.TestOrderDescription == updateDto.TestOrderDescription &&
                (request as UpdateTestOrderCommand)!.Status == updateDto.Status
            ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task UpdateTestOrder_ShouldReturnUnauthorized_WhenUserIdIsInvalid()
        {
            // Arrange
            var testOrderId = Guid.NewGuid();
            var updateDto = new UpdateTestOrderDto(
                null,
                null,
                "Updated description",
                StatusTestOrder.Completed
            );

            // Setup controller context without user ID
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var actionResult = await _controller.UpdateTestOrder(testOrderId, updateDto, CancellationToken.None);

            // Assert
            var unauthorizedResult = actionResult as UnauthorizedObjectResult;
            Assert.That(unauthorizedResult, Is.Not.Null);
            Assert.That(unauthorizedResult!.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));

            var errorResponse = unauthorizedResult.Value as ErrorResponse;
            Assert.Multiple(() =>
            {
                Assert.That(errorResponse, Is.Not.Null);
                Assert.That(errorResponse!.StatusCode, Is.EqualTo(401));
                Assert.That(errorResponse.Message, Is.EqualTo("Invalid user identifier."));
            });

            _mockSender.Verify(s => s.Send(It.IsAny<IRequest<TestOrder>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task UpdateTestOrder_ShouldSetTestOrderId_FromRouteParameter()
        {
            // Arrange
            var testOrderId = Guid.NewGuid();
            var updateById = Guid.NewGuid();
            var updateDto = new UpdateTestOrderDto(
                null,
                null,
                null,
                StatusTestOrder.Pending
            );

            var updatedTestOrder = new TestOrder
            {
                TestOrderId = testOrderId,
                PatientId = Guid.NewGuid(),
                Status = StatusTestOrder.Pending,
                CreateById = Guid.NewGuid(),
                CreateAt = DateTime.UtcNow,
                UpdateById = updateById,
                UpdateAt = DateTime.UtcNow
            };

            UpdateTestOrderCommand? capturedCommand = null;
            _mockSender.Setup(s => s.Send(It.IsAny<IRequest<TestOrder>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedTestOrder)
                .Callback<IRequest<TestOrder>, CancellationToken>((request, _) =>
                {
                    capturedCommand = request as UpdateTestOrderCommand;
                });

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _controller.ControllerContext.HttpContext.Request.Headers["X-User-Id"] = updateById.ToString();

            // Act
            await _controller.UpdateTestOrder(testOrderId, updateDto, CancellationToken.None);

            // Assert
            Assert.That(capturedCommand, Is.Not.Null);
            Assert.That(capturedCommand!.TestOrderId, Is.EqualTo(testOrderId));
        }

        [Test]
        public async Task UpdateTestOrder_ShouldSetUpdateById_FromUserHeader()
        {
            // Arrange
            var testOrderId = Guid.NewGuid();
            var updateById = Guid.NewGuid();
            var updateDto = new UpdateTestOrderDto(
                null,
                null,
                null,
                null
            );

            var updatedTestOrder = new TestOrder
            {
                TestOrderId = testOrderId,
                PatientId = Guid.NewGuid(),
                Status = StatusTestOrder.Pending,
                CreateById = Guid.NewGuid(),
                CreateAt = DateTime.UtcNow,
                UpdateById = updateById,
                UpdateAt = DateTime.UtcNow
            };

            UpdateTestOrderCommand? capturedCommand = null;
            _mockSender.Setup(s => s.Send(It.IsAny<IRequest<TestOrder>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedTestOrder)
                .Callback<IRequest<TestOrder>, CancellationToken>((request, _) =>
                {
                    capturedCommand = request as UpdateTestOrderCommand;
                });

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _controller.ControllerContext.HttpContext.Request.Headers["X-User-Id"] = updateById.ToString();

            // Act
            await _controller.UpdateTestOrder(testOrderId, updateDto, CancellationToken.None);

            // Assert
            Assert.That(capturedCommand, Is.Not.Null);
            Assert.That(capturedCommand!.UpdateById, Is.EqualTo(updateById));
        }

        [Test]
        public async Task UpdateTestOrder_ShouldHandlePartialUpdate()
        {
            // Arrange
            var testOrderId = Guid.NewGuid();
            var updateById = Guid.NewGuid();
            var updateDto = new UpdateTestOrderDto(
                null,
                null,
                "Only description updated",
                null
            );

            var updatedTestOrder = new TestOrder
            {
                TestOrderId = testOrderId,
                PatientId = Guid.NewGuid(),
                Status = StatusTestOrder.Pending,
                TestOrderDescription = "Only description updated",
                CreateById = Guid.NewGuid(),
                CreateAt = DateTime.UtcNow,
                UpdateById = updateById,
                UpdateAt = DateTime.UtcNow
            };

            _mockSender.Setup(s => s.Send(It.IsAny<IRequest<TestOrder>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedTestOrder);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _controller.ControllerContext.HttpContext.Request.Headers["X-User-Id"] = updateById.ToString();

            // Act
            var actionResult = await _controller.UpdateTestOrder(testOrderId, updateDto, CancellationToken.None);

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            _mockSender.Verify(s => s.Send(It.Is<IRequest<TestOrder>>(request =>
                request as UpdateTestOrderCommand != null &&
                (request as UpdateTestOrderCommand)!.TestOrderDescription == "Only description updated" &&
                (request as UpdateTestOrderCommand)!.RunById == null &&
                (request as UpdateTestOrderCommand)!.RunAt == null &&
                (request as UpdateTestOrderCommand)!.Status == null
            ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task UpdateTestOrder_ShouldMapDtoToCommand_Correctly()
        {
            // Arrange
            var testOrderId = Guid.NewGuid();
            var updateById = Guid.NewGuid();
            var runById = Guid.NewGuid();
            var runAt = DateTime.UtcNow;
            var description = "Test description";
            var status = StatusTestOrder.Completed;

            var updateDto = new UpdateTestOrderDto(
                runById,
                runAt,
                description,
                status
            );

            var updatedTestOrder = new TestOrder
            {
                TestOrderId = testOrderId,
                PatientId = Guid.NewGuid(),
                Status = status,
                TestOrderDescription = description,
                RunById = runById,
                RunAt = runAt,
                CreateById = Guid.NewGuid(),
                CreateAt = DateTime.UtcNow,
                UpdateById = updateById,
                UpdateAt = DateTime.UtcNow
            };

            UpdateTestOrderCommand? capturedCommand = null;
            _mockSender.Setup(s => s.Send(It.IsAny<IRequest<TestOrder>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedTestOrder)
                .Callback<IRequest<TestOrder>, CancellationToken>((request, _) =>
                {
                    capturedCommand = request as UpdateTestOrderCommand;
                });

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _controller.ControllerContext.HttpContext.Request.Headers["X-User-Id"] = updateById.ToString();

            // Act
            await _controller.UpdateTestOrder(testOrderId, updateDto, CancellationToken.None);

            // Assert
            Assert.That(capturedCommand, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(capturedCommand!.RunById, Is.EqualTo(runById));
                Assert.That(capturedCommand.RunAt, Is.EqualTo(runAt));
                Assert.That(capturedCommand.TestOrderDescription, Is.EqualTo(description));
                Assert.That(capturedCommand.Status, Is.EqualTo(status));
            });
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

        // ---------------------------------------------------------
        // Helper: Fake HttpContext with Claims
        // ---------------------------------------------------------
        private void SetUserWithClaim(string? nameIdentifier)
        {
            var claims = new List<Claim>();

            if (nameIdentifier != null)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, nameIdentifier));
            }

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };
        }

        // ---------------------------------------------------------
        // 1. SUCCESS CASE - RETURN 201
        // ---------------------------------------------------------
        [Test]
        public async Task CreateTestOrder_ShouldReturnCreated_WhenValid()
        {
            // Arrange
            var patientId = Guid.NewGuid();
            var createById = Guid.NewGuid().ToString();
            SetUserWithClaim(createById);

            var dto = new CreateTestOrderDto(null);

            var expected = new TestOrder
            {
                TestOrderId = Guid.NewGuid(),
                PatientId = patientId
            };

            CreateTestOrderCommand? capturedCmd = null;

            _mockSender
                .Setup(s => s.Send(It.IsAny<CreateTestOrderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected)
                .Callback<IRequest<TestOrder>, CancellationToken>((cmd, _) =>
                {
                    capturedCmd = cmd as CreateTestOrderCommand;
                });

            // Act
            var result = await _controller.CreateTestOrder(patientId, dto, CancellationToken.None);

            // Assert
            var created = result as ObjectResult;
            Assert.That(created, Is.Not.Null);
            Assert.That(created!.StatusCode, Is.EqualTo(StatusCodes.Status201Created));
            Assert.That(created.Value, Is.EqualTo(expected));

            // Validate mapped command
            Assert.That(capturedCmd, Is.Not.Null);
            Assert.That(capturedCmd!.PatientId, Is.EqualTo(patientId));
            Assert.That(capturedCmd.CreateById.ToString(), Is.EqualTo(createById));
        }

        // ---------------------------------------------------------
        // 2. MISSING CLAIM → Guid.TryParse FAIL → THROW INVALID OPERATION
        // ---------------------------------------------------------
        [Test]
        public void CreateTestOrder_ShouldThrow_WhenUserIdMissing()
        {
            // Arrange
            SetUserWithClaim(null); // no claim

            var patientId = Guid.NewGuid();
            var dto = new CreateTestOrderDto(null);

            // Act + Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _controller.CreateTestOrder(patientId, dto, CancellationToken.None));

            Assert.That(
                ex!.Message,
                Does.Contain("Can't parse 'createById' to Guid")
            );

            _mockSender.Verify(s => s.Send(It.IsAny<CreateTestOrderCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // ---------------------------------------------------------
        // 3. CLAIM NOT GUID → FAIL PARSE → THROW INVALID OPERATION
        // ---------------------------------------------------------
        [Test]
        public void CreateTestOrder_ShouldThrow_WhenUserIdInvalidGuid()
        {
            // Arrange
            SetUserWithClaim("not-a-guid");

            var patientId = Guid.NewGuid();
            var dto = new CreateTestOrderDto(null);

            // Act + Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _controller.CreateTestOrder(patientId, dto, CancellationToken.None));

            Assert.That(
                ex!.Message,
                Does.Contain("Can't parse 'createById' to Guid")
            );

            _mockSender.Verify(s => s.Send(It.IsAny<CreateTestOrderCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // ---------------------------------------------------------
        // 4. SENDER THROWS → CONTROLLER BUBBLES EXCEPTION
        // ---------------------------------------------------------
        [Test]
        public void CreateTestOrder_ShouldBubbleException_WhenHandlerFails()
        {
            // Arrange
            var patientId = Guid.NewGuid();
            var createById = Guid.NewGuid().ToString();
            SetUserWithClaim(createById);

            var dto = new CreateTestOrderDto(null);

            var expected = new Exception("Service crashed");

            _mockSender
                .Setup(s => s.Send(It.IsAny<CreateTestOrderCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expected);

            // Act + Assert
            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await _controller.CreateTestOrder(patientId, dto, CancellationToken.None)
            );

            Assert.That(ex!.Message, Is.EqualTo("Service crashed"));
        }

        [Test]
        public async Task DeleteTestOrder_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var id = Guid.NewGuid();

            _mockSender
                .Setup(s => s.Send(It.IsAny<DeleteTestOrderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteTestOrder(id, CancellationToken.None);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.StatusCode, Is.EqualTo(200));

            var response = okResult.Value as ApiResponse<object>;
            Assert.That(response, Is.Not.Null);

            var data = response!.Data;
            Assert.That(data, Is.Not.Null);

            // ---- READ PROPERTIES USING REFLECTION ----
            var type = data.GetType();

            var testOrderIdProp = type.GetProperty("TestOrderId");
            var deletedProp = type.GetProperty("Deleted");

            Assert.That(testOrderIdProp, Is.Not.Null);
            Assert.That(deletedProp, Is.Not.Null);

            var testOrderIdValue = (Guid)testOrderIdProp!.GetValue(data)!;
            var deletedValue = (bool)deletedProp!.GetValue(data)!;

            Assert.That(testOrderIdValue, Is.EqualTo(id));
            Assert.That(deletedValue, Is.True);

            // Verify command sent
            _mockSender.Verify(s =>
                    s.Send(It.Is<DeleteTestOrderCommand>(c => c.TestOrderId == id),
                        It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task DeleteTestOrder_ShouldReturnBadRequest_WhenResultIsFalse()
        {
            // Arrange
            var id = Guid.NewGuid();

            _mockSender
                .Setup(s => s.Send(It.IsAny<DeleteTestOrderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteTestOrder(id, CancellationToken.None);

            // Assert
            var badRequest = result as BadRequestObjectResult;
            Assert.That(badRequest, Is.Not.Null);
            Assert.That(badRequest!.StatusCode, Is.EqualTo(400));

            var error = badRequest.Value as ErrorResponse;
            Assert.That(error, Is.Not.Null);
            Assert.That(error!.StatusCode, Is.EqualTo(400));
            Assert.That(error.Message,
                Is.EqualTo("Cannot delete this TestOrder. Only Completed TestOrders can be deleted."));

            _mockSender.Verify(s =>
                    s.Send(It.Is<DeleteTestOrderCommand>(c => c.TestOrderId == id),
                        It.IsAny<CancellationToken>()),
                Times.Once);
        }
        [Test]
        public void DeleteTestOrder_ShouldThrowNotFoundException_WhenNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            var expected = new NotFoundException("Test order not found");

            _mockSender
                .Setup(s => s.Send(It.IsAny<DeleteTestOrderCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expected);

            // Act
            var ex = Assert.ThrowsAsync<NotFoundException>(() =>
                _controller.DeleteTestOrder(id, CancellationToken.None));

            // Assert
            Assert.That(ex!.Message, Is.EqualTo("Test order not found"));
        }
        [Test]
        public void DeleteTestOrder_ShouldThrowBusinessRuleException_WhenStatusInvalid()
        {
            // Arrange
            var id = Guid.NewGuid();
            var expected = new BusinessRuleException(
                "Cannot delete",
                "Only completed test orders can be deleted."
            );

            _mockSender
                .Setup(s => s.Send(It.IsAny<DeleteTestOrderCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expected);

            // Act
            var ex = Assert.ThrowsAsync<BusinessRuleException>(() =>
                _controller.DeleteTestOrder(id, CancellationToken.None));

            // Assert
            Assert.That(ex!.Message, Is.EqualTo("Only completed test orders can be deleted."));
            Assert.That(ex.Title, Is.EqualTo("Cannot delete"));
        }
        [Test]
        public void DeleteTestOrder_ShouldBubbleUnexpectedException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var expected = new Exception("Unexpected error");

            _mockSender
                .Setup(s => s.Send(It.IsAny<DeleteTestOrderCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expected);

            // Act
            var ex = Assert.ThrowsAsync<Exception>(() =>
                _controller.DeleteTestOrder(id, CancellationToken.None));

            // Assert
            Assert.That(ex!.Message, Is.EqualTo("Unexpected error"));
        }
    }
}
