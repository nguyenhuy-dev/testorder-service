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
                PatientId = Guid.NewGuid()
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
                PatientId = Guid.Empty
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
    }
}
