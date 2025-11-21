using FluentAssertions;
using TestOrderService.Application.Features.TestOrders.Commands.UpdateComment;
namespace TestOrderService.Application.Test.Features.TestOrders.Commands.UpdateComment
{
    [TestFixture]
    public class UpdateCommentCommandValidatorTests
    {

        [SetUp]
        public void SetUp()
        {
            _validator = new UpdateCommentCommandValidator();
        }
        private UpdateCommentCommandValidator _validator;

        [Test]
        public void Validate_WithValidTestOrderId_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new UpdateCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                CommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = "Valid content"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().NotContain(e => e.PropertyName == nameof(UpdateCommentCommand.TestOrderId));
        }

        [Test]
        public void Validate_WithEmptyTestOrderId_ShouldHaveValidationError()
        {
            // Arrange
            var command = new UpdateCommentCommand
            {
                TestOrderId = Guid.Empty,
                CommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = "Valid content"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(UpdateCommentCommand.TestOrderId) &&
                e.ErrorMessage == "Test order ID is required.");
        }

        [Test]
        public void Validate_WithValidCommentId_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new UpdateCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                CommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = "Valid content"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().NotContain(e => e.PropertyName == nameof(UpdateCommentCommand.CommentId));
        }

        [Test]
        public void Validate_WithEmptyCommentId_ShouldHaveValidationError()
        {
            // Arrange
            var command = new UpdateCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                CommentId = Guid.Empty,
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = "Valid content"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(UpdateCommentCommand.CommentId) &&
                e.ErrorMessage == "Comment ID is required.");
        }

        [Test]
        public void Validate_WithValidUserId_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new UpdateCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                CommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = "Valid content"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().NotContain(e => e.PropertyName == nameof(UpdateCommentCommand.UserId));
        }

        [Test]
        public void Validate_WithEmptyUserId_ShouldHaveValidationError()
        {
            // Arrange
            var command = new UpdateCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                CommentId = Guid.NewGuid(),
                UserId = Guid.Empty,
                Name = "Dr. Smith",
                Content = "Valid content"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(UpdateCommentCommand.UserId) &&
                e.ErrorMessage == "User ID is required.");
        }

        [Test]
        public void Validate_WithValidContent_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new UpdateCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                CommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = "This is valid updated content"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().NotContain(e => e.PropertyName == nameof(UpdateCommentCommand.Content));
        }

        [Test]
        public void Validate_WithEmptyContent_ShouldHaveValidationError()
        {
            // Arrange
            var command = new UpdateCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                CommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = string.Empty
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(UpdateCommentCommand.Content) &&
                e.ErrorMessage == "Content is required.");
        }

        [Test]
        public void Validate_WithNullContent_ShouldHaveValidationError()
        {
            // Arrange
            var command = new UpdateCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                CommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = null!
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(UpdateCommentCommand.Content) &&
                e.ErrorMessage == "Content is required.");
        }

        [Test]
        public void Validate_WithWhitespaceContent_ShouldHaveValidationError()
        {
            // Arrange
            var command = new UpdateCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                CommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = "   "
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(UpdateCommentCommand.Content) &&
                e.ErrorMessage == "Content is required.");
        }

        [Test]
        public void Validate_WithContentExceeding2000Characters_ShouldHaveValidationError()
        {
            // Arrange
            var command = new UpdateCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                CommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = new string('A', 2001) // 2001 characters
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(UpdateCommentCommand.Content) &&
                e.ErrorMessage == "Content must not exceed 2000 characters.");
        }

        [Test]
        public void Validate_WithContentExactly2000Characters_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new UpdateCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                CommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = new string('A', 2000) // Exactly 2000 characters
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().NotContain(e => e.PropertyName == nameof(UpdateCommentCommand.Content));
        }

        [Test]
        public void Validate_WithContentOneCharacter_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new UpdateCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                CommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = "A"
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
            var command = new UpdateCommentCommand
            {
                TestOrderId = Guid.Empty,
                CommentId = Guid.Empty,
                UserId = Guid.Empty,
                Name = "Dr. Smith",
                Content = string.Empty
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(4);
            result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateCommentCommand.TestOrderId));
            result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateCommentCommand.CommentId));
            result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateCommentCommand.UserId));
            result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateCommentCommand.Content));
        }

        [Test]
        public void Validate_WithContentEmptyAndTooLong_ShouldOnlyShowEmptyError()
        {
            // Arrange
            var command = new UpdateCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                CommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = string.Empty
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Where(e => e.PropertyName == nameof(UpdateCommentCommand.Content))
                .Should().ContainSingle()
                .Which.ErrorMessage.Should().Be("Content is required.");
        }

        [Test]
        public void Validate_WithOnlyTestOrderIdInvalid_ShouldHaveSingleValidationError()
        {
            // Arrange
            var command = new UpdateCommentCommand
            {
                TestOrderId = Guid.Empty,
                CommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = "Valid content"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.PropertyName.Should().Be(nameof(UpdateCommentCommand.TestOrderId));
        }

        [Test]
        public void Validate_WithOnlyCommentIdInvalid_ShouldHaveSingleValidationError()
        {
            // Arrange
            var command = new UpdateCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                CommentId = Guid.Empty,
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = "Valid content"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.PropertyName.Should().Be(nameof(UpdateCommentCommand.CommentId));
        }

        [Test]
        public void Validate_WithAllFieldsValid_ShouldPass()
        {
            // Arrange
            var command = new UpdateCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                CommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Dr. John Smith",
                Content = "Updated: Patient shows excellent response. All vitals are stable."
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Test]
        public void Validate_WithMaxLengthContent_ShouldPass()
        {
            // Arrange
            var command = new UpdateCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                CommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = new string('X', 2000)
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Test]
        public void Validate_WithContentContainingNewLines_ShouldBeValid()
        {
            // Arrange
            var command = new UpdateCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                CommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = "Line 1\nLine 2\nLine 3"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Test]
        public void Validate_WithContentContainingTabs_ShouldBeValid()
        {
            // Arrange
            var command = new UpdateCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                CommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = "Column1\tColumn2\tColumn3"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Test]
        public void Validate_WithSpecialCharacters_ShouldBeValid()
        {
            // Arrange
            var command = new UpdateCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                CommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = "Special chars: @#$%^&*()_+-=[]{}|;':\"<>?,./"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Test]
        public void Validate_WithUnicodeCharacters_ShouldBeValid()
        {
            // Arrange
            var command = new UpdateCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                CommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = "Unicode: 你好世界 مرحبا العالم שלום עולם"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Test]
        public void Validate_WithEmojis_ShouldBeValid()
        {
            // Arrange
            var command = new UpdateCommentCommand
            {
                TestOrderId = Guid.NewGuid(),
                CommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Dr. Smith",
                Content = "Patient is doing great! 😊👍✨"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }
    }
}
