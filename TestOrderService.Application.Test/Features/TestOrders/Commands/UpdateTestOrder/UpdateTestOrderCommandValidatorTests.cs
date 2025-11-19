using FluentValidation.TestHelper;
using TestOrderService.Application.Features.TestOrders.Commands.UpdateTestOrder;
using TestOrderService.Domain.Entities;
namespace TestOrderService.Application.Test.Features.TestOrders.Commands.UpdateTestOrder
{
    /// <summary>
    ///     Unit test for UpdateTestOrderCommandValidator
    /// </summary>
    [TestFixture]
    public class UpdateTestOrderCommandValidatorTests
    {
        /// <summary>
        ///     Setups this instance.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _validator = new UpdateTestOrderCommandValidator();
        }

        /// <summary>
        ///     The validator
        /// </summary>
        private UpdateTestOrderCommandValidator _validator = null!;

        /// <summary>
        ///     Validate should pass when command is valid.
        /// </summary>
        [Test]
        public void Validate_ShouldPass_WhenCommandIsValid()
        {
            // Arrange
            var command = new UpdateTestOrderCommand
            {
                TestOrderId = Guid.NewGuid(),
                UpdateById = Guid.NewGuid(),
                UpdateAt = DateTime.UtcNow
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        /// <summary>
        ///     Validate should fail when test order id is empty.
        /// </summary>
        [Test]
        public void Validate_ShouldFail_WhenTestOrderIdIsEmpty()
        {
            // Arrange
            var command = new UpdateTestOrderCommand
            {
                TestOrderId = Guid.Empty,
                UpdateById = Guid.NewGuid(),
                UpdateAt = DateTime.UtcNow
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.TestOrderId)
                .WithErrorMessage("TestOrderId is required.");
        }

        /// <summary>
        ///     Validate should fail when update by id is empty.
        /// </summary>
        [Test]
        public void Validate_ShouldFail_WhenUpdateByIdIsEmpty()
        {
            // Arrange
            var command = new UpdateTestOrderCommand
            {
                TestOrderId = Guid.NewGuid(),
                UpdateById = Guid.Empty,
                UpdateAt = DateTime.UtcNow
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UpdateById)
                .WithErrorMessage("UpdateById is required.");
        }

        /// <summary>
        ///     Validate should pass when optional fields are null.
        /// </summary>
        [Test]
        public void Validate_ShouldPass_WhenOptionalFieldsAreNull()
        {
            // Arrange
            var command = new UpdateTestOrderCommand
            {
                TestOrderId = Guid.NewGuid(),
                UpdateById = Guid.NewGuid(),
                UpdateAt = DateTime.UtcNow,
                RunById = null,
                RunAt = null,
                TestOrderDescription = null,
                Status = null
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        /// <summary>
        ///     Validate should pass when optional fields have values.
        /// </summary>
        [Test]
        public void Validate_ShouldPass_WhenOptionalFieldsHaveValues()
        {
            // Arrange
            var command = new UpdateTestOrderCommand
            {
                TestOrderId = Guid.NewGuid(),
                UpdateById = Guid.NewGuid(),
                UpdateAt = DateTime.UtcNow,
                RunById = Guid.NewGuid(),
                RunAt = DateTime.UtcNow,
                TestOrderDescription = "Test description",
                Status = StatusTestOrder.Completed
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        /// <summary>
        ///     Validate should pass when run by id is provided without run at.
        /// </summary>
        [Test]
        public void Validate_ShouldPass_WhenRunByIdIsProvidedWithoutRunAt()
        {
            // Arrange
            var command = new UpdateTestOrderCommand
            {
                TestOrderId = Guid.NewGuid(),
                UpdateById = Guid.NewGuid(),
                UpdateAt = DateTime.UtcNow,
                RunById = Guid.NewGuid(),
                RunAt = null
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        /// <summary>
        ///     Validate should pass when run at is provided without run by id.
        /// </summary>
        [Test]
        public void Validate_ShouldPass_WhenRunAtIsProvidedWithoutRunById()
        {
            // Arrange
            var command = new UpdateTestOrderCommand
            {
                TestOrderId = Guid.NewGuid(),
                UpdateById = Guid.NewGuid(),
                UpdateAt = DateTime.UtcNow,
                RunById = null,
                RunAt = DateTime.UtcNow
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        /// <summary>
        ///     Validate should pass when test order description is empty string.
        /// </summary>
        [Test]
        public void Validate_ShouldPass_WhenTestOrderDescriptionIsEmptyString()
        {
            // Arrange
            var command = new UpdateTestOrderCommand
            {
                TestOrderId = Guid.NewGuid(),
                UpdateById = Guid.NewGuid(),
                UpdateAt = DateTime.UtcNow,
                TestOrderDescription = ""
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
