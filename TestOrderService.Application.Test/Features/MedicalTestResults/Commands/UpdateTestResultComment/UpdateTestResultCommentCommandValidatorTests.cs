using FluentAssertions;
using TestOrderService.Application.Features.MedicalTestResults.Commands.UpdateTestResultComment;
namespace TestOrderService.Application.Test.Features.MedicalTestResults.Commands.UpdateTestResultComment
{
    [TestFixture]
    public class UpdateTestResultCommentCommandValidatorTests
    {
        [SetUp]
        public void SetUp()
        {
            _validator = new UpdateTestResultCommentCommandValidator();
        }
        private UpdateTestResultCommentCommandValidator _validator;

        [Test]
        public void Validate_WithValidRequest_ShouldPass()
        {
            // Arrange
            var command = new UpdateTestResultCommentCommand
            {
                TestResultId = Guid.NewGuid(),
                CommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Content = "Valid content"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Test]
        public void Validate_WithEmptyTestResultId_ShouldFail()
        {
            // Arrange
            var command = new UpdateTestResultCommentCommand
            {
                TestResultId = Guid.Empty,
                CommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Content = "Valid content"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateTestResultCommentCommand.TestResultId));
        }

        [Test]
        public void Validate_WithEmptyCommentId_ShouldFail()
        {
            // Arrange
            var command = new UpdateTestResultCommentCommand
            {
                TestResultId = Guid.NewGuid(),
                CommentId = Guid.Empty,
                UserId = Guid.NewGuid(),
                Content = "Valid content"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateTestResultCommentCommand.CommentId));
        }

        [Test]
        public void Validate_WithEmptyContent_ShouldFail()
        {
            // Arrange
            var command = new UpdateTestResultCommentCommand
            {
                TestResultId = Guid.NewGuid(),
                CommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Content = string.Empty
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateTestResultCommentCommand.Content));
        }
    }
}
