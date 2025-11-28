using TestOrderService.Domain.Entities;
namespace TestOrderService.Application.Interfaces
{
    /// <summary>
    ///     Repository interface for TestResultComment entity operations
    /// </summary>
    public interface ITestResultCommentRepository
    {
        /// <summary>
        ///     Creates a new test result comment asynchronously
        /// </summary>
        /// <param name="comment">The test result comment entity</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The created test result comment</returns>
        Task<TestResultComment> CreateTestResultCommentAsync(TestResultComment comment, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Gets a test result comment by ID asynchronously
        /// </summary>
        /// <param name="commentId">The test result comment ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The test result comment if found, null otherwise</returns>
        Task<TestResultComment?> GetTestResultCommentByIdAsync(Guid commentId, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Gets a test result comment by ID and test result ID asynchronously
        /// </summary>
        /// <param name="commentId">The test result comment ID</param>
        /// <param name="testResultId">The test result ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The test result comment if found, null otherwise</returns>
        Task<TestResultComment?> GetTestResultCommentByIdAndTestResultIdAsync(
            Guid commentId,
            Guid testResultId,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Gets all comments for a test result asynchronously
        /// </summary>
        /// <param name="testResultId">The test result ID</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of test result comments</returns>
        Task<(List<TestResultComment> Comments, int TotalCount)> GetTestResultCommentsByTestResultIdAsync(
            Guid testResultId,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Updates a test result comment asynchronously
        /// </summary>
        /// <param name="comment">The test result comment to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The updated test result comment</returns>
        Task<TestResultComment> UpdateTestResultCommentAsync(TestResultComment comment, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Deletes a test result comment asynchronously
        /// </summary>
        /// <param name="comment">The test result comment to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task DeleteTestResultCommentAsync(TestResultComment comment, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Checks if a test result exists
        /// </summary>
        /// <param name="testResultId">The test result ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if exists, false otherwise</returns>
        Task<bool> TestResultExistsAsync(Guid testResultId, CancellationToken cancellationToken = default);
    }
}
