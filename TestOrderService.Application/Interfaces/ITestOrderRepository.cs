using TestOrderService.Application.DTOs;
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
        /// <summary>
        ///     Gets all test orders asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<IEnumerable<TestOrder>> GetAllTestOrdersAsync(CancellationToken cancellationToken);

        /// <summary>
        ///     Gets the by id using the specified id
        /// </summary>
        /// <param name="id">The id</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task containing the test order</returns>
        Task<TestOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Deletes the test order
        /// </summary>
        /// <param name="testOrder">The test order</param>
        void Delete(TestOrder testOrder);
        /// <summary>
        ///     Gets the test orders asynchronous.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<PaginatedList<TestOrderDto>> GetTestOrdersAsync(GetTestOrdersRequest request, CancellationToken cancellationToken = default);

        Task<TestOrder?> GetTestOrderByIdNotIncludeAsync(Guid id, CancellationToken cancellationToken);

        Task<TestOrder> UpdateTestOrderToCompleted(Guid testOrderId, CancellationToken cancellationToken);
    }
}
