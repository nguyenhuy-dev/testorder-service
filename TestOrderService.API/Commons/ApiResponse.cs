namespace TestOrderService.API.Commons
{
    /// <summary>
    ///     Api response template.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        ///     Gets or sets the status code.
        /// </summary>
        /// <value>
        ///     The status code.
        /// </value>
        public int StatusCode { get; set; }

        /// <summary>
        ///     Gets or sets the message.
        /// </summary>
        /// <value>
        ///     The message.
        /// </value>
        public string? Message { get; set; }

        /// <summary>
        ///     Gets or sets the data.
        /// </summary>
        /// <value>
        ///     The data.
        /// </value>
        public T? Data { get; set; }

        /// <summary>
        ///     Successes the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="message">The message.</param>
        /// <param name="statusCode">The status code.</param>
        /// <returns></returns>
        public static ApiResponse<T> Success(T? data, string message = "Request successful.", int statusCode = StatusCodes.Status200OK)
        {
            return new ApiResponse<T>
            {
                StatusCode = statusCode,
                Message = message,
                Data = data
            };
        }
    }
}
