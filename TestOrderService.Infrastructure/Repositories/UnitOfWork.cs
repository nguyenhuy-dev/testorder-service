using TestOrderService.Application.Interfaces;
using TestOrderService.Infrastructure.Data;
namespace TestOrderService.Infrastructure.Repositories
{
    /// <summary>
    ///     Unit of work implement.
    /// </summary>
    /// <seealso cref="TestOrderService.Application.Interfaces.IUnitOfWork" />
    public class UnitOfWork(TestOrderServiceDbContext dbContext) : IUnitOfWork
    {
        /// <summary>
        ///     The database context
        /// </summary>
        private readonly TestOrderServiceDbContext _dbContext = dbContext;

        /// <summary>
        ///     Saves the changes asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
