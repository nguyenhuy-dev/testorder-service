using FluentAssertions;
using TestOrderService.Application.Features.MedicalTestResults.Commands.DeleteTestResultComment;
namespace TestOrderService.Application.Test.Features.MedicalTestResults.Commands.DeleteTestResultComment
{
    [TestFixture]
    public class DeleteTestResultCommentCommandValidatorTests
    {
        [SetUp]
        public void SetUp()
        {
            _validator = new DeleteTestResultCommentCommandValidator();
        }
        private DeleteTestResultCommentCommandValidator _validator;

        [Test]
        public void Validate_WithValidRequest_ShouldPass()
        {
            // Arrange
            var command = new DeleteTestResultCommentCommand
            {
                TestResultId = Guid.NewGuid(),
                CommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Role = "Doctor"
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
            var command = new DeleteTestResultCommentCommand
            {
                TestResultId = Guid.Empty,
                CommentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Role = "Doctor"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(DeleteTestResultCommentCommand.TestResultId));
        }

        [Test]
        public void Validate_WithEmptyCommentId_ShouldFail()
        {
            // Arrange
            var command = new DeleteTestResultCommentCommand
            {
                TestResultId = Guid.NewGuid(),
                CommentId = Guid.Empty,
                UserId = Guid.NewGuid(),
                Role = "Doctor"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(DeleteTestResultCommentCommand.CommentId));
        }
    }
}
