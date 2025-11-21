using TestOrderService.Domain.Entities;
namespace TestOrderService.Application.Interfaces
{
    /// <summary>
    ///     Repository interface for Comment entity operations
    /// </summary>
    public interface ICommentRepository
    {
        /// <summary>
        ///     Creates a new comment asynchronously
        /// </summary>
        /// <param name="comment">The comment entity</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The created comment</returns>
        Task<Comment> CreateCommentAsync(Comment comment, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Gets a comment by ID asynchronously
        /// </summary>
        /// <param name="commentId">The comment ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The comment if found, null otherwise</returns>
        Task<Comment?> GetCommentByIdAsync(Guid commentId, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Gets a comment by ID and test order ID asynchronously
        /// </summary>
        /// <param name="commentId">The comment ID</param>
        /// <param name="testOrderId">The test order ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The comment if found, null otherwise</returns>
        Task<Comment?> GetCommentByIdAndTestOrderIdAsync(
            Guid commentId,
            Guid testOrderId,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Gets all comments for a test order asynchronously
        /// </summary>
        /// <param name="testOrderId">The test order ID</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of comments</returns>
        Task<(List<Comment> Comments, int TotalCount)> GetCommentsByTestOrderIdAsync(
            Guid testOrderId,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Updates a comment asynchronously
        /// </summary>
        /// <param name="comment">The comment to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The updated comment</returns>
        Task<Comment> UpdateCommentAsync(Comment comment, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Deletes a comment asynchronously
        /// </summary>
        /// <param name="comment">The comment to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task DeleteCommentAsync(Comment comment, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Checks if a test order exists
        /// </summary>
        /// <param name="testOrderId">The test order ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if exists, false otherwise</returns>
        Task<bool> TestOrderExistsAsync(Guid testOrderId, CancellationToken cancellationToken = default);
    }
}
