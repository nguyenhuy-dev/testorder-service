using TestOrderService.Domain.Entities;
namespace TestOrderService.Application.DTOs
{
    /// <summary>
    ///     Request model for getting test orders with pagination and filtering
    /// </summary>
    public class GetTestOrdersRequest
    {
        /// <summary>
        ///     Filter by test order status (optional)
        /// </summary>
        public StatusTestOrder? Status { get; set; }

        /// <summary>
        ///     Page number for pagination (default: 1)
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        ///     Number of items per page (default: 10)
        /// </summary>
        public int PageSize { get; set; } = 10;
    }
}
