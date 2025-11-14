using TestOrderService.Domain.Entities;
namespace TestOrderService.Application.Interfaces
{
    /// <summary>
    ///     Interface for test order repository.
    /// </summary>
    public interface ITestOrderRepository
    {
        /// <summary>
        ///     Creates the test order.
        /// </summary>
        /// <param name="testOrder">The test order.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<TestOrder> CreateTestOrderAsync(TestOrder testOrder, CancellationToken cancellationToken);

        Task<IEnumerable<TestOrder>> GetAllTestOrdersAsync(CancellationToken cancellationToken);
    }
}
