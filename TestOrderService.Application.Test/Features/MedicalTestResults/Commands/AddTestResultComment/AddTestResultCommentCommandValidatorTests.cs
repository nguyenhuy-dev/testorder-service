using FluentAssertions;
using TestOrderService.Application.Features.MedicalTestResults.Commands.AddTestResultComment;
namespace TestOrderService.Application.Test.Features.MedicalTestResults.Commands.AddTestResultComment
{
    [TestFixture]
    public class AddTestResultCommentCommandValidatorTests
    {
        [SetUp]
        public void SetUp()
        {
            _validator = new AddTestResultCommentCommandValidator();
        }
        private AddTestResultCommentCommandValidator _validator;

        [Test]
        public void Validate_WithValidTestResultId_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new AddTestResultCommentCommand
            {
                TestResultId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Content = "Valid content",
                Name = "Dr. Smith"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().NotContain(e => e.PropertyName == nameof(AddTestResultCommentCommand.TestResultId));
        }

        [Test]
        public void Validate_WithEmptyTestResultId_ShouldHaveValidationError()
        {
            // Arrange
            var command = new AddTestResultCommentCommand
            {
                TestResultId = Guid.Empty,
                UserId = Guid.NewGuid(),
                Content = "Valid content",
                Name = "Dr. Smith"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(AddTestResultCommentCommand.TestResultId) &&
                e.ErrorMessage == "Test result ID is required.");
        }

        [Test]
        public void Validate_WithValidContent_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new AddTestResultCommentCommand
            {
                TestResultId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Content = "This is valid content",
                Name = "Dr. Smith"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().NotContain(e => e.PropertyName == nameof(AddTestResultCommentCommand.Content));
        }

        [Test]
        public void Validate_WithEmptyContent_ShouldHaveValidationError()
        {
            // Arrange
            var command = new AddTestResultCommentCommand
            {
                TestResultId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Content = string.Empty,
                Name = "Dr. Smith"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(AddTestResultCommentCommand.Content) &&
                e.ErrorMessage == "Content is required.");
        }

        [Test]
        public void Validate_WithContentExceeding2000Characters_ShouldHaveValidationError()
        {
            // Arrange
            var command = new AddTestResultCommentCommand
            {
                TestResultId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Content = new string('A', 2001), // 2001 characters
                Name = "Dr. Smith"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(AddTestResultCommentCommand.Content) &&
                e.ErrorMessage == "Content must not exceed 2000 characters.");
        }
    }
}
