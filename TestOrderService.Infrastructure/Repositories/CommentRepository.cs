using Microsoft.EntityFrameworkCore;
using TestOrderService.Application.Interfaces;
using TestOrderService.Domain.Entities;
using TestOrderService.Infrastructure.Data;
namespace TestOrderService.Infrastructure.Repositories
{
    /// <summary>
    ///     Repository implementation for Comment entity operations
    /// </summary>
    public class CommentRepository : ICommentRepository
    {
        private readonly TestOrderServiceDbContext _context;

        public CommentRepository(TestOrderServiceDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<Comment> CreateCommentAsync(
            Comment comment,
            CancellationToken cancellationToken = default)
        {
            await _context.Comments.AddAsync(comment, cancellationToken);
            return comment;
        }

        /// <inheritdoc />
        public async Task<Comment?> GetCommentByIdAsync(
            Guid commentId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Comments
                .FirstOrDefaultAsync(c => c.CommentId == commentId, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Comment?> GetCommentByIdAndTestOrderIdAsync(
            Guid commentId,
            Guid testOrderId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Comments
                .FirstOrDefaultAsync(
                    c => c.CommentId == commentId && c.TestOrderId == testOrderId,
                    cancellationToken);
        }

        /// <inheritdoc />
        public async Task<(List<Comment> Comments, int TotalCount)> GetCommentsByTestOrderIdAsync(
            Guid testOrderId,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Comments
                .Where(c => c.TestOrderId == testOrderId);

            var totalCount = await query.CountAsync(cancellationToken);

            var comments = await query
                .OrderByDescending(c => c.CreateAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (comments, totalCount);
        }

        /// <inheritdoc />
        public async Task<Comment> UpdateCommentAsync(
            Comment comment,
            CancellationToken cancellationToken = default)
        {
            _context.Comments.Update(comment);
            return await Task.FromResult(comment);
        }

        /// <inheritdoc />
        public async Task DeleteCommentAsync(
            Comment comment,
            CancellationToken cancellationToken = default)
        {
            _context.Comments.Remove(comment);
            await Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task<bool> TestOrderExistsAsync(
            Guid testOrderId,
            CancellationToken cancellationToken = default)
        {
            return await _context.TestOrders
                .AnyAsync(t => t.TestOrderId == testOrderId, cancellationToken);
        }
    }
}
