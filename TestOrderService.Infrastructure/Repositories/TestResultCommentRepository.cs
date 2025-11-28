using Microsoft.EntityFrameworkCore;
using TestOrderService.Application.Interfaces;
using TestOrderService.Domain.Entities;
using TestOrderService.Infrastructure.Data;
namespace TestOrderService.Infrastructure.Repositories
{
    /// <summary>
    ///     Repository implementation for TestResultComment entity operations
    /// </summary>
    public class TestResultCommentRepository(TestOrderServiceDbContext context) : ITestResultCommentRepository
    {

        /// <inheritdoc />
        public async Task<TestResultComment> CreateTestResultCommentAsync(
            TestResultComment comment,
            CancellationToken cancellationToken = default)
        {
            await context.TestResultComments.AddAsync(comment, cancellationToken);
            return comment;
        }

        /// <inheritdoc />
        public async Task<TestResultComment?> GetTestResultCommentByIdAsync(
            Guid commentId,
            CancellationToken cancellationToken = default)
        {
            return await context.TestResultComments
                .FirstOrDefaultAsync(c => c.TestResultCommentId == commentId, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<TestResultComment?> GetTestResultCommentByIdAndTestResultIdAsync(
            Guid commentId,
            Guid testResultId,
            CancellationToken cancellationToken = default)
        {
            return await context.TestResultComments
                .FirstOrDefaultAsync(
                    c => c.TestResultCommentId == commentId && c.TestResultId == testResultId,
                    cancellationToken);
        }

        /// <inheritdoc />
        public async Task<(List<TestResultComment> Comments, int TotalCount)> GetTestResultCommentsByTestResultIdAsync(
            Guid testResultId,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = context.TestResultComments
                .Where(c => c.TestResultId == testResultId);

            var totalCount = await query.CountAsync(cancellationToken);

            var comments = await query
                .OrderByDescending(c => c.CreateAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (comments, totalCount);
        }

        /// <inheritdoc />
        public async Task<TestResultComment> UpdateTestResultCommentAsync(
            TestResultComment comment,
            CancellationToken cancellationToken = default)
        {
            context.TestResultComments.Update(comment);
            return await Task.FromResult(comment);
        }

        /// <inheritdoc />
        public async Task DeleteTestResultCommentAsync(
            TestResultComment comment,
            CancellationToken cancellationToken = default)
        {
            context.TestResultComments.Remove(comment);
            await Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task<bool> TestResultExistsAsync(
            Guid testResultId,
            CancellationToken cancellationToken = default)
        {
            return await context.TestResults
                .AnyAsync(t => t.TestResultId == testResultId, cancellationToken);
        }
    }
}
