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
    }
}
