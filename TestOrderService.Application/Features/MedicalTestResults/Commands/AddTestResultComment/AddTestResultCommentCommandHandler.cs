using Mapster;
using TestOrderService.Application.DTOs;
using TestOrderService.Application.Exceptions;
using TestOrderService.Application.Interfaces;
using TestOrderService.Application.Interfaces.gRPC;
using TestOrderService.Application.Interfaces.Message;
using TestOrderService.Domain.Entities;
namespace TestOrderService.Application.Features.MedicalTestResults.Commands.AddTestResultComment
{
    /// <summary>
    ///     Command handler for adding a comment to a test result
    /// </summary>
    /// <seealso cref="ICommandHandler{AddTestResultCommentCommand,TestResultCommentDto}" />
    public class AddTestResultCommentCommandHandler(
        ITestResultCommentRepository testResultCommentRepository,
        IUnitOfWork unitOfWork,
        IUserGrpcClient userGrpcClient) : ICommandHandler<AddTestResultCommentCommand, TestResultCommentDto>
    {
        /// <summary>
        ///     The test result comment repository
        /// </summary>
        private readonly ITestResultCommentRepository _testResultCommentRepository = testResultCommentRepository;

        /// <summary>
        ///     The unit of work
        /// </summary>
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        /// <summary>
        ///     The user gRPC client
        /// </summary>
        private readonly IUserGrpcClient _userGrpcClient = userGrpcClient;

        /// <summary>
        ///     Handles the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.NotFoundException">Thrown when test result not found</exception>
        /// <exception cref="System.UnauthorizedAccessException">Thrown when user not found in IAM service</exception>
        /// <exception cref="ForbiddenException">Thrown when user lacks permission</exception>
        public async Task<TestResultCommentDto> Handle(
            AddTestResultCommentCommand request,
            CancellationToken cancellationToken)
        {
            // Verify test result exists
            var testResultExists = await _testResultCommentRepository.TestResultExistsAsync(
                request.TestResultId,
                cancellationToken);

            if (!testResultExists)
            {
                throw new NotFoundException(nameof(TestResult), request.TestResultId);
            }

            // Check if user has authorized role (Not implemented)

            var comment = new TestResultComment
            {
                TestResultCommentId = Guid.NewGuid(),
                Content = request.Content,
                CreateById = request.UserId,
                CreateByName = request.Name,
                CreateAt = DateTime.UtcNow,
                TestResultId = request.TestResultId
            };

            // Save to database
            var createdComment = await _testResultCommentRepository.CreateTestResultCommentAsync(comment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Map to response and add user name
            var response = createdComment.Adapt<TestResultCommentDto>();

            return response;
        }
    }
}
