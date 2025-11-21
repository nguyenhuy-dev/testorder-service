using FluentAssertions;
using TestOrderService.Application.Features.TestOrders.Commands.AddComment;
namespace TestOrderService.Application.Test.Features.TestOrders.Commands.AddComment
{
    [TestFixture]
    public class AddCommentCommandValidatorTests
    {

        [SetUp]
        public void SetUp()
        {
            _validator = new AddCommentCommandValidator();
        }
        private AddCommentCommandValidator _validator;

        [Test]
        public void Validate_WithValidTestOrderId_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new AddCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Content = "Valid content",
                Name = "Dr. Smith"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().NotContain(e => e.PropertyName == nameof(AddCommentCommand.TestOrderId));
        }

        [Test]
        public void Validate_WithEmptyTestOrderId_ShouldHaveValidationError()
        {
            // Arrange
            var command = new AddCommentCommand
            {
                TestOrderId = Guid.Empty,
                UserId = Guid.NewGuid(),
                Content = "Valid content",
                Name = "Dr. Smith"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(AddCommentCommand.TestOrderId) &&
                e.ErrorMessage == "Test order ID is required.");
        }

        [Test]
        public void Validate_WithValidUserId_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new AddCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Content = "Valid content",
                Name = "Dr. Smith"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().NotContain(e => e.PropertyName == nameof(AddCommentCommand.UserId));
        }

        [Test]
        public void Validate_WithEmptyUserId_ShouldHaveValidationError()
        {
            // Arrange
            var command = new AddCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                UserId = Guid.Empty,
                Content = "Valid content",
                Name = "Dr. Smith"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(AddCommentCommand.UserId) &&
                e.ErrorMessage == "User ID is required.");
        }

        [Test]
        public void Validate_WithValidContent_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new AddCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Content = "This is valid content",
                Name = "Dr. Smith"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().NotContain(e => e.PropertyName == nameof(AddCommentCommand.Content));
        }

        [Test]
        public void Validate_WithEmptyContent_ShouldHaveValidationError()
        {
            // Arrange
            var command = new AddCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Content = string.Empty,
                Name = "Dr. Smith"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(AddCommentCommand.Content) &&
                e.ErrorMessage == "Content is required.");
        }

        [Test]
        public void Validate_WithNullContent_ShouldHaveValidationError()
        {
            // Arrange
            var command = new AddCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Content = null!,
                Name = "Dr. Smith"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(AddCommentCommand.Content) &&
                e.ErrorMessage == "Content is required.");
        }

        [Test]
        public void Validate_WithWhitespaceContent_ShouldHaveValidationError()
        {
            // Arrange
            var command = new AddCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Content = "   ",
                Name = "Dr. Smith"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(AddCommentCommand.Content) &&
                e.ErrorMessage == "Content is required.");
        }

        [Test]
        public void Validate_WithContentExceeding2000Characters_ShouldHaveValidationError()
        {
            // Arrange
            var command = new AddCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Content = new string('A', 2001), // 2001 characters
                Name = "Dr. Smith"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(AddCommentCommand.Content) &&
                e.ErrorMessage == "Content must not exceed 2000 characters.");
        }

        [Test]
        public void Validate_WithContentExactly2000Characters_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new AddCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Content = new string('A', 2000), // Exactly 2000 characters
                Name = "Dr. Smith"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().NotContain(e => e.PropertyName == nameof(AddCommentCommand.Content));
        }

        [Test]
        public void Validate_WithContentOneCharacter_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new AddCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Content = "A",
                Name = "Dr. Smith"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Test]
        public void Validate_WithAllFieldsInvalid_ShouldHaveMultipleValidationErrors()
        {
            // Arrange
            var command = new AddCommentCommand
            {
                TestOrderId = Guid.Empty,
                UserId = Guid.Empty,
                Content = string.Empty,
                Name = "Dr. Smith"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(3);
            result.Errors.Should().Contain(e => e.PropertyName == nameof(AddCommentCommand.TestOrderId));
            result.Errors.Should().Contain(e => e.PropertyName == nameof(AddCommentCommand.UserId));
            result.Errors.Should().Contain(e => e.PropertyName == nameof(AddCommentCommand.Content));
        }

        [Test]
        public void Validate_WithContentEmptyAndTooLong_ShouldOnlyShowEmptyError()
        {
            // Arrange
            var command = new AddCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Content = string.Empty,
                Name = "Dr. Smith"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Where(e => e.PropertyName == nameof(AddCommentCommand.Content))
                .Should().ContainSingle()
                .Which.ErrorMessage.Should().Be("Content is required.");
        }

        [Test]
        public void Validate_WithAllFieldsValid_ShouldPass()
        {
            // Arrange
            var command = new AddCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Content = "Patient shows excellent response. All vitals are stable.",
                Name = "Dr. John Smith"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }
    }
}
