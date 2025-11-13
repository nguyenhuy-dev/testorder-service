namespace TestOrderService.Application.Interfaces
{
    /// <summary>
    ///     Interface for unit of work.
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        ///     Saves the changes asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
