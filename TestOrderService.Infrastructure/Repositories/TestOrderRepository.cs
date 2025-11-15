using Microsoft.EntityFrameworkCore;
using TestOrderService.Application.Interfaces;
using TestOrderService.Domain.Entities;
using TestOrderService.Infrastructure.Data;
namespace TestOrderService.Infrastructure.Repositories
{
    /// <summary>
    ///     Test order repository implement.
    /// </summary>
    /// <seealso cref="TestOrderService.Application.Interfaces.ITestOrderRepository" />
    public class TestOrderRepository(TestOrderServiceDbContext dbContext) : ITestOrderRepository
    {
        /// <summary>
        ///     The database context
        /// </summary>
        private readonly TestOrderServiceDbContext _dbContext = dbContext;

        /// <summary>
        ///     Creates the test order.
        /// </summary>
        /// <param name="testOrder">The test order.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<TestOrder> CreateTestOrderAsync(TestOrder testOrder, CancellationToken cancellationToken)
        {
            await _dbContext.AddAsync(testOrder, cancellationToken);

            return testOrder;
        }

        /// <summary>
        ///     Gets the all test orders using the specified cancellation token
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task containing an enumerable of test order</returns>
        public async Task<IEnumerable<TestOrder>> GetAllTestOrdersAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.TestOrders.ToArrayAsync(cancellationToken);
        }
        /// <summary>
        ///     Gets the by id using the specified id
        /// </summary>
        /// <param name="id">The id</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task containing the test order</returns>
        public async Task<TestOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.TestOrders
                .FirstOrDefaultAsync(t => t.TestOrderId == id, cancellationToken);
        }

        /// <summary>
        ///     Delete TestOrder (hard delete).
        /// </summary>
        public void Delete(TestOrder entity)
        {
            _dbContext.TestOrders.Remove(entity);
        }
    }
}
