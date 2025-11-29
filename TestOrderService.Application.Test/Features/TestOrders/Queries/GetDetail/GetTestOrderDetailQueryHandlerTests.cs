using FluentAssertions;
using Moq;
using TestOrderService.Application.DTOs.gRPC.GetAllPatients;
using TestOrderService.Application.DTOs.gRPC.GetAllTestDefinitions;
using TestOrderService.Application.DTOs.gRPC.GetAllUsers;
using TestOrderService.Application.Exceptions;
using TestOrderService.Application.Features.TestOrders.Queries.GetDetail;
using TestOrderService.Application.Interfaces;
using TestOrderService.Application.Interfaces.gRPC;
using TestOrderService.Domain.Entities;
namespace TestOrderService.Application.Test.Features.TestOrders.Queries.GetDetail
{
    [TestFixture]
    public class GetTestOrderDetailQueryHandlerTests
    {

        [SetUp]
        public void Setup()
        {
            _repoMock = new Mock<ITestOrderRepository>();
            _patientGrpcMock = new Mock<IPatientGrpcClient>();
            _userGrpcMock = new Mock<IUserGrpcClient>();
            _testDefGrpcMock = new Mock<ITestDefinitionGrpcClient>();

            _handler = new GetTestOrderDetailQueryHandler(
                _repoMock.Object,
                _patientGrpcMock.Object,
                _userGrpcMock.Object,
                _testDefGrpcMock.Object
            );
        }
        private Mock<ITestOrderRepository> _repoMock = null!;
        private Mock<IPatientGrpcClient> _patientGrpcMock = null!;
        private Mock<IUserGrpcClient> _userGrpcMock = null!;
        private Mock<ITestDefinitionGrpcClient> _testDefGrpcMock = null!;
        private GetTestOrderDetailQueryHandler _handler = null!;

        // 1. TestOrder not found
        [Test]
        public void Handle_ShouldThrow_NotFound_WhenOrderMissing()
        {
            var id = Guid.NewGuid();

            _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TestOrder?)null);

            Func<Task> act = async () =>
                await _handler.Handle(new GetTestOrderDetailQuery(id), CancellationToken.None);

            act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"TestOrder with id '{id}' was not found.");
        }

        // 2. Patient not found
        [Test]
        public void Handle_ShouldThrow_NotFound_WhenPatientMissing()
        {
            var order = new TestOrder
            {
                TestOrderId = Guid.NewGuid(),
                PatientId = Guid.NewGuid()
            };

            _repoMock.Setup(r => r.GetByIdAsync(order.TestOrderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            _patientGrpcMock.Setup(p => p.GetAllPatients(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PatientDto>());

            Func<Task> act = async () =>
                await _handler.Handle(new GetTestOrderDetailQuery(order.TestOrderId), CancellationToken.None);

            act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Patient with id '{order.PatientId}' was not found.");
        }

        // 3. createdBy UNKNOWN
        [Test]
        public async Task Handle_ShouldReturn_Unknown_WhenCreatedByNotFound()
        {
            var order = new TestOrder
            {
                TestOrderId = Guid.NewGuid(),
                PatientId = Guid.NewGuid(),
                CreateById = Guid.NewGuid()
            };

            var patient = new PatientDto
            {
                PatientId = order.PatientId,
                DateOfBirth = new DateOnly(2000, 1, 1)
            };

            _repoMock.Setup(r => r.GetByIdAsync(order.TestOrderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            _patientGrpcMock.Setup(p => p.GetAllPatients(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PatientDto> { patient });

            _userGrpcMock.Setup(u => u.GetAllUsers(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserDto>()); // none found

            _testDefGrpcMock.Setup(t => t.GetAllTestDefinitions(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TestDefinitionDto>());

            var result = await _handler.Handle(new GetTestOrderDetailQuery(order.TestOrderId), CancellationToken.None);

            result.CreatedBy.Should().Be("Unknown");
        }

        // 4. RunBy & ReviewBy null
        [Test]
        public async Task Handle_ShouldReturn_NullRunBy_And_NullReviewBy()
        {
            var creatorId = Guid.NewGuid();

            var order = new TestOrder
            {
                TestOrderId = Guid.NewGuid(),
                PatientId = Guid.NewGuid(),
                CreateById = creatorId,
                RunById = Guid.Empty,
                ReviewId = null
            };

            var patient = new PatientDto
            {
                PatientId = order.PatientId,
                DateOfBirth = new DateOnly(2000, 1, 1)
            };

            var creator = new UserDto
            {
                UserId = creatorId,
                FullName = "Doctor A"
            };

            _repoMock.Setup(x => x.GetByIdAsync(order.TestOrderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            _patientGrpcMock.Setup(x => x.GetAllPatients(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PatientDto> { patient });

            _userGrpcMock.Setup(x => x.GetAllUsers(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserDto> { creator });

            _testDefGrpcMock.Setup(x => x.GetAllTestDefinitions(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TestDefinitionDto>());

            var result = await _handler.Handle(new GetTestOrderDetailQuery(order.TestOrderId), CancellationToken.None);

            result.RunBy.Should().BeNull();
            result.ReviewBy.Should().BeNull();
        }

        // 5. TestResult → Definition NOT found
        [Test]
        public async Task Handle_ShouldReturn_TestResult_WithNullNames_WhenDefinitionMissing()
        {
            var order = new TestOrder
            {
                TestOrderId = Guid.NewGuid(),
                PatientId = Guid.NewGuid(),
                CreateById = Guid.NewGuid(),
                TestResults = new List<TestResult>
                {
                    new TestResult
                    {
                        TestResultId = Guid.NewGuid(),
                        TestDefinitionId = 999,
                        Value = 5.3 // double
                    }
                }
            };

            var patient = new PatientDto
            {
                PatientId = order.PatientId,
                DateOfBirth = new DateOnly(1990, 1, 1)
            };

            _repoMock.Setup(r => r.GetByIdAsync(order.TestOrderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            _patientGrpcMock.Setup(p => p.GetAllPatients(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PatientDto> { patient });

            _userGrpcMock.Setup(u => u.GetAllUsers(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserDto>());

            _testDefGrpcMock.Setup(t => t.GetAllTestDefinitions(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TestDefinitionDto>());

            var result = await _handler.Handle(new GetTestOrderDetailQuery(order.TestOrderId), CancellationToken.None);

            result.TestResults.First().TestName.Should().BeNull();
            result.TestResults.First().Unit.Should().BeNull();
        }

        // 6. Definition found
        [Test]
        public async Task Handle_ShouldMap_TestResult_WithDefinition()
        {
            var order = new TestOrder
            {
                TestOrderId = Guid.NewGuid(),
                PatientId = Guid.NewGuid(),
                CreateById = Guid.NewGuid(),
                TestResults = new List<TestResult>
                {
                    new TestResult
                    {
                        TestResultId = Guid.NewGuid(),
                        TestDefinitionId = 1,
                        Value = 10.2 // double
                    }
                }
            };

            var patient = new PatientDto
            {
                PatientId = order.PatientId,
                DateOfBirth = new DateOnly(1995, 5, 10)
            };

            var creator = new UserDto
            {
                UserId = order.CreateById,
                FullName = "Dr A"
            };

            var def = new TestDefinitionDto
            {
                TestDefinitionId = 1,
                TestName = "Glucose",
                Unit = "mg/dL"
            };

            _repoMock.Setup(r => r.GetByIdAsync(order.TestOrderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            _patientGrpcMock.Setup(p => p.GetAllPatients(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PatientDto> { patient });

            _userGrpcMock.Setup(u => u.GetAllUsers(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserDto> { creator });

            _testDefGrpcMock.Setup(t => t.GetAllTestDefinitions(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TestDefinitionDto> { def });

            var result = await _handler.Handle(new GetTestOrderDetailQuery(order.TestOrderId), CancellationToken.None);

            result.TestResults.First().TestName.Should().Be("Glucose");
            result.TestResults.First().Unit.Should().Be("mg/dL");
        }

        // 7. Comments mapping
        [Test]
        public async Task Handle_ShouldMap_Comments()
        {
            var c = new Comment
            {
                CommentId = Guid.NewGuid(),
                Content = "Hello world",
                CreateByName = "Nurse",
                CreateAt = DateTime.UtcNow
            };

            var order = new TestOrder
            {
                TestOrderId = Guid.NewGuid(),
                PatientId = Guid.NewGuid(),
                Comments = new List<Comment> { c }
            };

            var patient = new PatientDto
            {
                PatientId = order.PatientId,
                DateOfBirth = new DateOnly(1990, 1, 1)
            };

            _repoMock.Setup(r => r.GetByIdAsync(order.TestOrderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            _patientGrpcMock.Setup(p => p.GetAllPatients(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PatientDto> { patient });

            _userGrpcMock.Setup(u => u.GetAllUsers(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserDto>());

            _testDefGrpcMock.Setup(t => t.GetAllTestDefinitions(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TestDefinitionDto>());

            var result = await _handler.Handle(new GetTestOrderDetailQuery(order.TestOrderId), CancellationToken.None);

            result.Comments.Should().HaveCount(1);
            result.Comments.First().Content.Should().Be("Hello world");
        }

        // 8. Empty comment list
        [Test]
        public async Task Handle_ShouldReturn_EmptyComments_WhenNone()
        {
            var order = new TestOrder
            {
                TestOrderId = Guid.NewGuid(),
                PatientId = Guid.NewGuid(),
                Comments = new List<Comment>()
            };

            var patient = new PatientDto
            {
                PatientId = order.PatientId,
                DateOfBirth = new DateOnly(1990, 1, 1)
            };

            _repoMock.Setup(r => r.GetByIdAsync(order.TestOrderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            _patientGrpcMock.Setup(p => p.GetAllPatients(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PatientDto> { patient });

            _userGrpcMock.Setup(u => u.GetAllUsers(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserDto>());

            _testDefGrpcMock.Setup(t => t.GetAllTestDefinitions(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TestDefinitionDto>());

            var result = await _handler.Handle(new GetTestOrderDetailQuery(order.TestOrderId), CancellationToken.None);

            result.Comments.Should().BeEmpty();
        }

        // 9. Age calculation
        [Test]
        public async Task Handle_ShouldCalculate_AgeCorrectly()
        {
            var order = new TestOrder
            {
                TestOrderId = Guid.NewGuid(),
                PatientId = Guid.NewGuid()
            };

            var birth = new DateOnly(DateTime.Today.Year - 25, DateTime.Today.Month, DateTime.Today.Day);

            var patient = new PatientDto
            {
                PatientId = order.PatientId,
                DateOfBirth = birth
            };

            _repoMock.Setup(r => r.GetByIdAsync(order.TestOrderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            _patientGrpcMock.Setup(p => p.GetAllPatients(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PatientDto> { patient });

            _userGrpcMock.Setup(u => u.GetAllUsers(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserDto>());

            _testDefGrpcMock.Setup(d => d.GetAllTestDefinitions(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TestDefinitionDto>());

            var result = await _handler.Handle(new GetTestOrderDetailQuery(order.TestOrderId), CancellationToken.None);

            result.Age.Should().Be(25);
        }
    }
}
