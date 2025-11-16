using FluentAssertions;
using NSubstitute;
using TestOrderService.Application.DTOs;
using TestOrderService.Application.Features.TestOrders.Queries.GetTestOrders;
using TestOrderService.Application.Interfaces;
using TestOrderService.Domain.Entities;
namespace TestOrderService.Application.Test.Features.TestOrders.Queries.GetTestOrders
{
    /// <summary>
    ///     Unit test for GetTestOrdersQueryHandler
    /// </summary>
    [TestFixture]
    public class GetTestOrdersQueryHandlerTests
    {

        /// <summary>
        ///     Setups this instance.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _testOrderRepository = Substitute.For<ITestOrderRepository>();
            _handler = new GetTestOrdersQueryHandler(_testOrderRepository);
        }
        /// <summary>
        ///     The test order repository
        /// </summary>
        private ITestOrderRepository _testOrderRepository = null!;

        /// <summary>
        ///     The handler
        /// </summary>
        private GetTestOrdersQueryHandler _handler = null!;

        /// <summary>
        ///     Handle returns paginated list from repository.
        /// </summary>
        [Test]
        public async Task Handle_ReturnsPaginatedList_FromRepository()
        {
            // Arrange
            var request = new GetTestOrdersRequest
            {
                PageNumber = 1,
                PageSize = 10
            };
            var query = new GetTestOrdersQuery(request);

            var items = new List<TestOrderDto>
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
            };

            var paginated = new PaginatedList<TestOrderDto>(items, items.Count, 1, 10);

            _testOrderRepository.GetTestOrdersAsync(Arg.Any<GetTestOrdersRequest>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(paginated));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(10);
            await _testOrderRepository.Received(1).GetTestOrdersAsync(Arg.Any<GetTestOrdersRequest>(), Arg.Any<CancellationToken>());
        }

        /// <summary>
        ///     Handle passes query to repository.
        /// </summary>
        [Test]
        public async Task Handle_PassesQuery_ToRepository()
        {
            // Arrange
            var request = new GetTestOrdersRequest
            {
                PageNumber = 2,
                PageSize = 5,
                Status = StatusTestOrder.Completed
            };
            var query = new GetTestOrdersQuery(request);

            var empty = new PaginatedList<TestOrderDto>(new List<TestOrderDto>(), 0, 2, 5);
            _testOrderRepository.GetTestOrdersAsync(Arg.Any<GetTestOrdersRequest>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(empty));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            await _testOrderRepository.Received(1).GetTestOrdersAsync(Arg.Any<GetTestOrdersRequest>(), Arg.Any<CancellationToken>());
        }

        /// <summary>
        ///     Handle returns empty list when no orders exist.
        /// </summary>
        [Test]
        public async Task Handle_ReturnsEmptyList_WhenNoOrdersExist()
        {
            // Arrange
            var request = new GetTestOrdersRequest
            {
                PageNumber = 1,
                PageSize = 10
            };
            var query = new GetTestOrdersQuery(request);

            var empty = new PaginatedList<TestOrderDto>(new List<TestOrderDto>(), 0, 1, 10);
            _testOrderRepository.GetTestOrdersAsync(Arg.Any<GetTestOrdersRequest>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(empty));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        /// <summary>
        ///     Handle filters by status correctly.
        /// </summary>
        [Test]
        public async Task Handle_FiltersByStatus_Correctly()
        {
            // Arrange
            var request = new GetTestOrdersRequest
            {
                PageNumber = 1,
                PageSize = 10,
                Status = StatusTestOrder.Pending
            };
            var query = new GetTestOrdersQuery(request);

            var items = new List<TestOrderDto>
            {
                new TestOrderDto
                {
                    TestOrderId = Guid.NewGuid(),
                    PatientId = Guid.NewGuid(),
                    FullName = "Test Patient",
                    Age = 30,
                    Phone = "0123456789",
                    Gender = false,
                    Status = "Pending",
                    CreateAt = DateTime.UtcNow,
                    CreateById = Guid.NewGuid(),
                    CreateByName = "Dr. Smith",
                    RunAt = null,
                    RunById = Guid.Empty,
                    RunByName = null
                }
            };

            var paginated = new PaginatedList<TestOrderDto>(items, items.Count, 1, 10);
            _testOrderRepository.GetTestOrdersAsync(Arg.Any<GetTestOrdersRequest>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(paginated));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().OnlyContain(x => x.Status == "Pending");
        }

        /// <summary>
        ///     Handle returns correct pagination info.
        /// </summary>
        [Test]
        public async Task Handle_ReturnsCorrectPaginationInfo()
        {
            // Arrange
            var request = new GetTestOrdersRequest
            {
                PageNumber = 2,
                PageSize = 5
            };
            var query = new GetTestOrdersQuery(request);

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
                    Status = "Completed",
                    CreateAt = DateTime.UtcNow,
                    CreateById = Guid.NewGuid(),
                    CreateByName = "Doctor",
                    RunAt = DateTime.UtcNow,
                    RunById = Guid.NewGuid(),
                    RunByName = "Lab Tech"
                });
            }

            var paginated = new PaginatedList<TestOrderDto>(items, 15, 2, 5);
            _testOrderRepository.GetTestOrdersAsync(Arg.Any<GetTestOrdersRequest>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(paginated));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Items.Should().HaveCount(5);
            result.TotalCount.Should().Be(15);
            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalPages.Should().Be(3);
        }
    }
}
