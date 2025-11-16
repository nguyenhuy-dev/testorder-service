namespace TestOrderService.Application.DTOs
{
    /// <summary>
    ///     Generic paginated list for API responses
    /// </summary>
    /// <typeparam name="T">Type of items in the list</typeparam>
    public class PaginatedList<T>
    {

        /// <summary>
        ///     Initializes a new instance of the <see cref="PaginatedList{T}" /> class.
        /// </summary>
        public PaginatedList()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PaginatedList{T}" /> class.
        /// </summary>
        /// <param name="items">The items for the current page.</param>
        /// <param name="totalCount">The total count of items.</param>
        /// <param name="pageNumber">The current page number.</param>
        /// <param name="pageSize">The page size.</param>
        public PaginatedList(List<T> items, int totalCount, int pageNumber, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
        /// <summary>
        ///     List of items for the current page
        /// </summary>
        public List<T> Items { get; set; } = new List<T>();

        /// <summary>
        ///     Total number of items across all pages
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        ///     Current page number
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        ///     Number of items per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        ///     Total number of pages calculated from TotalCount and PageSize
        /// </summary>
        public int TotalPages
        {
            get => (int)Math.Ceiling(TotalCount / (double)PageSize);
        }
    }
}
