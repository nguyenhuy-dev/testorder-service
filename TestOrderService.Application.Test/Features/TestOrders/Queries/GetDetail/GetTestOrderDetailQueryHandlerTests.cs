using Moq;
using TestOrderService.Application.DTOs.gRPC.GetAllPatients;
using TestOrderService.Application.DTOs.gRPC.GetAllUsers;
using TestOrderService.Application.Exceptions;
using TestOrderService.Application.Features.TestOrders.Queries.GetDetail;
using TestOrderService.Application.Interfaces;
using TestOrderService.Application.Interfaces.gRPC;
using TestOrderService.Domain.Entities;
namespace TestOrderService.Application.Test.Features.TestOrders.Queries.GetDetail
{
    /// <summary>
    ///     Unit test for GetTestOrderDetailQueryHandler
    /// </summary>
    [TestFixture]
    public class GetTestOrderDetailQueryHandlerTests
    {

        /// <summary>
        ///     Setups this instance.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _repoMock = new Mock<ITestOrderRepository>();
            _patientGrpcMock = new Mock<IPatientGrpcClient>();
            _userGrpcMock = new Mock<IUserGrpcClient>();

            _handler = new GetTestOrderDetailQueryHandler(
                _repoMock.Object,
                _patientGrpcMock.Object,
                _userGrpcMock.Object
            );
        }
        /// <summary>
        ///     The repo mock
        /// </summary>
        private Mock<ITestOrderRepository> _repoMock = null!;
        /// <summary>
        ///     The patient GRPC mock
        /// </summary>
        private Mock<IPatientGrpcClient> _patientGrpcMock = null!;
        /// <summary>
        ///     The user GRPC mock
        /// </summary>
        private Mock<IUserGrpcClient> _userGrpcMock = null!;
        /// <summary>
        ///     The handler
        /// </summary>
        private GetTestOrderDetailQueryHandler _handler = null!;

        /// <summary>
        ///     Handles the should throw not found when test order not found.
        /// </summary>
        [Test]
        public void Handle_ShouldThrowNotFound_WhenTestOrderNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();

            _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TestOrder?)null);

            var query = new GetTestOrderDetailQuery(id);

            // Act + Assert
            var ex = Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(query, CancellationToken.None));
            Assert.That(ex!.Message, Is.EqualTo($"TestOrder with id '{id}' was not found."));
        }

        /// <summary>
        ///     Handles the should throw not found when patient not found.
        /// </summary>
        [Test]
        public void Handle_ShouldThrowNotFound_WhenPatientNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            var testOrder = new TestOrder
            {
                TestOrderId = id,
                PatientId = Guid.NewGuid(),
                CreateById = Guid.NewGuid()
            };

            _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(testOrder);

            _patientGrpcMock.Setup(p => p.GetAllPatients(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PatientDto>()); // Empty list = not found

            var query = new GetTestOrderDetailQuery(id);

            // Act + Assert
            var ex = Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(query, CancellationToken.None));
            Assert.That(ex!.Message, Is.EqualTo($"Patient with id '{testOrder.PatientId}' was not found."));
        }

        /// <summary>
        ///     Handles the should return detail dto when valid.
        /// </summary>
        [Test]
        public async Task Handle_ShouldReturnDetailDto_WhenValid()
        {
            // Arrange
            var id = Guid.NewGuid();
            var patientId = Guid.NewGuid();
            var creatorId = Guid.NewGuid();

            var testOrder = new TestOrder
            {
                TestOrderId = id,
                PatientId = patientId,
                CreateById = creatorId,
                Status = StatusTestOrder.Pending,
                CreateAt = DateTime.UtcNow,
                TestOrderDescription = "Blood Test"
            };

            var patient = new PatientDto
            {
                PatientId = patientId,
                PatientName = "Nguyen Van A",
                PhoneNumber = "0901234567",
                Gender = true,
                Address = "HCM",
                DateOfBirth = new DateOnly(2000, 1, 1)
            };

            var user = new UserDto
            {
                UserId = creatorId,
                FullName = "Doctor John"
            };

            _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(testOrder);

            _patientGrpcMock.Setup(p => p.GetAllPatients(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PatientDto> { patient });

            _userGrpcMock.Setup(u => u.GetAllUsers(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserDto> { user });

            var query = new GetTestOrderDetailQuery(id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.TestOrderId, Is.EqualTo(id));
                Assert.That(result.PatientId, Is.EqualTo(patientId));
                Assert.That(result.PatientName, Is.EqualTo("Nguyen Van A"));
                Assert.That(result.CreatedBy, Is.EqualTo("Doctor John"));
                Assert.That(result.Status, Is.EqualTo("Pending"));
            });
        }

        /// <summary>
        ///     Handles the should compute correct age.
        /// </summary>
        [Test]
        public async Task Handle_ShouldComputeCorrectAge()
        {
            // Arrange
            var id = Guid.NewGuid();
            var patientId = Guid.NewGuid();

            var birthdate = new DateOnly(DateTime.Today.Year - 30, 1, 1); // 30 years old

            var testOrder = new TestOrder
            {
                TestOrderId = id,
                PatientId = patientId
            };

            var patient = new PatientDto
            {
                PatientId = patientId,
                PatientName = "Test",
                DateOfBirth = birthdate
            };

            _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(testOrder);

            _patientGrpcMock.Setup(p => p.GetAllPatients(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PatientDto> { patient });

            _userGrpcMock.Setup(u => u.GetAllUsers(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserDto>());

            var query = new GetTestOrderDetailQuery(id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result.Age, Is.EqualTo(30));
        }

        /// <summary>
        ///     Handles the should include and map comments when test order has comments.
        /// </summary>
        [Test]
        public async Task Handle_ShouldIncludeMappedComments_WhenTestOrderHasComments()
        {
            // Arrange
            var id = Guid.NewGuid();
            var patientId = Guid.NewGuid();
            var comment1Id = Guid.NewGuid();
            var comment2Id = Guid.NewGuid();

            // 1. Create Comments
            var comment1 = new Comment
            {
                CommentId = comment1Id,
                Content = "Patient looks pale",
                CreateByName = "Nurse Joy",
                CreateAt = DateTime.UtcNow.AddHours(-2),
                TestOrderId = id
            };

            var comment2 = new Comment
            {
                CommentId = comment2Id,
                Content = "Vital signs stable",
                CreateByName = "Dr. House",
                CreateAt = DateTime.UtcNow.AddHours(-1),
                TestOrderId = id
            };

            // 2. Create TestOrder with the comments attached
            var testOrder = new TestOrder
            {
                TestOrderId = id,
                PatientId = patientId,
                Comments = new List<Comment> { comment1, comment2 }
            };

            // 3. Setup Patient (Required to pass the NotFound check before the loop)
            var patient = new PatientDto
            {
                PatientId = patientId,
                PatientName = "Test Patient",
                DateOfBirth = new DateOnly(1990, 1, 1)
            };

            _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(testOrder);

            _patientGrpcMock.Setup(p => p.GetAllPatients(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PatientDto> { patient });

            _userGrpcMock.Setup(u => u.GetAllUsers(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserDto>()); // Users irrelevant for this test, return empty

            var query = new GetTestOrderDetailQuery(id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Comments, Is.Not.Null);
                Assert.That(result.Comments.Count, Is.EqualTo(2));

                // Verify mapping accuracy for first comment
                var firstMapped = result.Comments.FirstOrDefault(c => c.CommentId == comment1Id);
                Assert.That(firstMapped, Is.Not.Null);
                Assert.That(firstMapped!.Content, Is.EqualTo("Patient looks pale"));
                Assert.That(firstMapped.CreateByName, Is.EqualTo("Nurse Joy"));

                // Verify mapping accuracy for second comment
                var secondMapped = result.Comments.FirstOrDefault(c => c.CommentId == comment2Id);
                Assert.That(secondMapped, Is.Not.Null);
                Assert.That(secondMapped!.Content, Is.EqualTo("Vital signs stable"));
            });
        }

        /// <summary>
        ///     Handles the should return empty comment list when no comments exist.
        /// </summary>
        [Test]
        public async Task Handle_ShouldReturnEmptyCommentList_WhenNoCommentsExist()
        {
            // Arrange
            var id = Guid.NewGuid();
            var patientId = Guid.NewGuid();

            var testOrder = new TestOrder
            {
                TestOrderId = id,
                PatientId = patientId,
                Comments = new List<Comment>() // Empty List
            };

            var patient = new PatientDto
            {
                PatientId = patientId,
                DateOfBirth = new DateOnly(1990, 1, 1)
            };

            _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(testOrder);

            _patientGrpcMock.Setup(p => p.GetAllPatients(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PatientDto> { patient });

            _userGrpcMock.Setup(u => u.GetAllUsers(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserDto>());

            var query = new GetTestOrderDetailQuery(id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Comments, Is.Not.Null);
                Assert.That(result.Comments, Is.Empty);
            });
        }
    }
}
