using TestOrderService.Application.Features.TestOrders.Commands.DeleteTestOrder;
namespace TestOrderService.Application.Test.Features.TestOrders.Commands.DeleteTestOrder
{
    [TestFixture]
    public class DeleteTestOrderCommandValidatorTest
    {

        [SetUp]
        public void Setup()
        {
            _validator = new DeleteTestOrderCommandValidator();
        }
        private DeleteTestOrderCommandValidator _validator;

        [Test]
        public void Validator_Should_Pass_For_Valid_Id()
        {
            // Arrange
            var command = new DeleteTestOrderCommand(Guid.NewGuid());

            // Act
            var result = _validator.Validate(command);

            // Assert
            if (!result.IsValid)
                Assert.Fail("Expected validator to pass for a valid command.");

            Assert.Pass();
        }

        [Test]
        public void Validator_Should_Fail_For_Empty_Id()
        {
            // Arrange
            var command = new DeleteTestOrderCommand(Guid.Empty);

            // Act
            var result = _validator.Validate(command);

            // Assert
            if (result.IsValid)
                Assert.Fail("Expected validator to fail for empty TestOrderId.");

            if (!result.Errors.Exists(e => e.PropertyName == "TestOrderId"))
                Assert.Fail("Expected validation error for TestOrderId.");

            Assert.Pass();
        }
    }
}
