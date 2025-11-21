using FluentAssertions;
using TestOrderService.Application.Features.TestOrders.Commands.DeleteComment;
namespace TestOrderService.Application.Test.Features.TestOrders.Commands.DeleteComment
{
    [TestFixture]
    public class DeleteCommentCommandValidatorTests
    {

        [SetUp]
        public void SetUp()
        {
            _validator = new DeleteCommentCommandValidator();
        }
        private DeleteCommentCommandValidator _validator;

        [Test]
        public void Validate_WithValidTestOrderId_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new DeleteCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                CommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Role = "doctor"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().NotContain(e => e.PropertyName == nameof(DeleteCommentCommand.TestOrderId));
        }

        [Test]
        public void Validate_WithEmptyTestOrderId_ShouldHaveValidationError()
        {
            // Arrange
            var command = new DeleteCommentCommand
            {
                TestOrderId = Guid.Empty,
                CommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Role = "doctor"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(DeleteCommentCommand.TestOrderId) &&
                e.ErrorMessage == "Test order ID is required.");
        }

        [Test]
        public void Validate_WithValidCommentId_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new DeleteCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                CommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Role = "doctor"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().NotContain(e => e.PropertyName == nameof(DeleteCommentCommand.CommentId));
        }

        [Test]
        public void Validate_WithEmptyCommentId_ShouldHaveValidationError()
        {
            // Arrange
            var command = new DeleteCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                CommentId = Guid.Empty,
                UserId = Guid.NewGuid(),
                Role = "doctor"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(DeleteCommentCommand.CommentId) &&
                e.ErrorMessage == "Comment ID is required.");
        }

        [Test]
        public void Validate_WithValidUserId_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new DeleteCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                CommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Role = "doctor"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().NotContain(e => e.PropertyName == nameof(DeleteCommentCommand.UserId));
        }

        [Test]
        public void Validate_WithEmptyUserId_ShouldHaveValidationError()
        {
            // Arrange
            var command = new DeleteCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                CommentId = Guid.NewGuid(),
                UserId = Guid.Empty,
                Role = "doctor"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(DeleteCommentCommand.UserId) &&
                e.ErrorMessage == "User ID is required.");
        }

        [Test]
        public void Validate_WithAllFieldsInvalid_ShouldHaveMultipleValidationErrors()
        {
            // Arrange
            var command = new DeleteCommentCommand
            {
                TestOrderId = Guid.Empty,
                CommentId = Guid.Empty,
                UserId = Guid.Empty,
                Role = "doctor"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(3);
            result.Errors.Should().Contain(e => e.PropertyName == nameof(DeleteCommentCommand.TestOrderId));
            result.Errors.Should().Contain(e => e.PropertyName == nameof(DeleteCommentCommand.CommentId));
            result.Errors.Should().Contain(e => e.PropertyName == nameof(DeleteCommentCommand.UserId));
        }

        [Test]
        public void Validate_WithOnlyTestOrderIdInvalid_ShouldHaveSingleValidationError()
        {
            // Arrange
            var command = new DeleteCommentCommand
            {
                TestOrderId = Guid.Empty,
                CommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Role = "doctor"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.PropertyName.Should().Be(nameof(DeleteCommentCommand.TestOrderId));
        }

        [Test]
        public void Validate_WithOnlyCommentIdInvalid_ShouldHaveSingleValidationError()
        {
            // Arrange
            var command = new DeleteCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                CommentId = Guid.Empty,
                UserId = Guid.NewGuid(),
                Role = "doctor"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.PropertyName.Should().Be(nameof(DeleteCommentCommand.CommentId));
        }

        [Test]
        public void Validate_WithOnlyUserIdInvalid_ShouldHaveSingleValidationError()
        {
            // Arrange
            var command = new DeleteCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                CommentId = Guid.NewGuid(),
                UserId = Guid.Empty,
                Role = "doctor"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.PropertyName.Should().Be(nameof(DeleteCommentCommand.UserId));
        }

        [Test]
        public void Validate_WithAllFieldsValid_ShouldPass()
        {
            // Arrange
            var command = new DeleteCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                CommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Role = "admin"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Test]
        public void Validate_WithDoctorRole_ShouldPass()
        {
            // Arrange
            var command = new DeleteCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                CommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Role = "doctor"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Test]
        public void Validate_WithTechnicianRole_ShouldPass()
        {
            // Arrange
            var command = new DeleteCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                CommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Role = "technician"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Test]
        [TestCase("admin")]
        [TestCase("Admin")]
        [TestCase("ADMIN")]
        [TestCase("doctor")]
        [TestCase("Doctor")]
        [TestCase("technician")]
        [TestCase("Technician")]
        [TestCase("nurse")]
        [TestCase("")]
        public void Validate_WithVariousRoles_ShouldNotValidateRole(string role)
        {
            // Arrange
            var command = new DeleteCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                CommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Role = role!
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            // Validator does not validate Role field, only IDs
            result.Errors.Should().NotContain(e => e.PropertyName == nameof(DeleteCommentCommand.Role));
        }
    }
}
