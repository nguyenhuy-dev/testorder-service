using TestOrderService.Application.Features.TestOrders.Commands.CreateTestOrder;
namespace TestOrderService.Application.Test.Features.TestOrders.Commands.CreateTestOrder
{
    [TestFixture]
    public class CreateTestOrderCommandValidatorTests
    {

        [SetUp]
        public void Setup()
        {
            _validator = new CreateTestOrderCommandValidator();
        }
        private CreateTestOrderCommandValidator _validator;

        [Test]
        public void Validator_Should_Pass_For_Valid_Command()
        {
            // Arrange
            var command = new CreateTestOrderCommand
            {
                PatientId = Guid.NewGuid(),
                RunById = Guid.NewGuid(),
                RunAt = DateTime.UtcNow.AddMinutes(5) // future date
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            if (!result.IsValid)
                Assert.Fail("Expected validator to pass for valid command");

            Assert.Pass();
        }

        [Test]
        public void Validator_Should_Fail_When_PatientId_Empty()
        {
            // Arrange
            var command = new CreateTestOrderCommand
            {
                PatientId = Guid.Empty,
                RunById = Guid.NewGuid(),
                RunAt = DateTime.UtcNow.AddMinutes(5)
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            if (result.IsValid)
                Assert.Fail("Expected validation failure when PatientId is empty");

            if (!result.Errors.Exists(e => e.PropertyName == "PatientId"))
                Assert.Fail("Expected error for PatientId");

            Assert.Pass();
        }

        [Test]
        public void Validator_Should_Fail_When_RunById_Empty()
        {
            // Arrange
            var command = new CreateTestOrderCommand
            {
                PatientId = Guid.NewGuid(),
                RunById = Guid.Empty,
                RunAt = DateTime.UtcNow.AddMinutes(5)
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            if (result.IsValid)
                Assert.Fail("Expected validation failure when RunById is empty");

            if (!result.Errors.Exists(e => e.PropertyName == "RunById"))
                Assert.Fail("Expected error for RunById");

            Assert.Pass();
        }

        [Test]
        public void Validator_Should_Fail_When_RunAt_Not_Future()
        {
            // Arrange
            var command = new CreateTestOrderCommand
            {
                PatientId = Guid.NewGuid(),
                RunById = Guid.NewGuid(),
                RunAt = DateTime.UtcNow.AddMinutes(-5) // past date
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            if (result.IsValid)
                Assert.Fail("Expected validation failure when RunAt is not in the future");

            if (!result.Errors.Exists(e => e.PropertyName == "RunAt"))
                Assert.Fail("Expected error for RunAt");

            Assert.Pass();
        }

        [Test]
        public void Validator_Should_Fail_When_RunAt_Empty_Default()
        {
            // Arrange
            var command = new CreateTestOrderCommand
            {
                PatientId = Guid.NewGuid(),
                RunById = Guid.NewGuid(),
                RunAt = default // 01/01/0001
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            if (result.IsValid)
                Assert.Fail("Expected validation failure when RunAt is default (empty)");

            if (!result.Errors.Exists(e => e.PropertyName == "RunAt"))
                Assert.Fail("Expected error for RunAt");

            Assert.Pass();
        }
    }
}
