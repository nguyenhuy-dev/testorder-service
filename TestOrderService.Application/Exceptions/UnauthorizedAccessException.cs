namespace TestOrderService.Application.Exceptions
{
    /// <summary>
    ///     Unauthorized access exception.
    /// </summary>
    /// <seealso cref="TestOrderService.Application.Exceptions.ApplicationException" />
    public class UnauthorizedAccessException : ApplicationException
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="UnauthorizedAccessException" /> class.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        public UnauthorizedAccessException(string title, string message) : base(title, message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UnauthorizedAccessException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public UnauthorizedAccessException(string message) : base("Unauthorized access", message) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UnauthorizedAccessException" /> class.
        /// </summary>
        public UnauthorizedAccessException() : base("Unauthorized access", "User is not identified.") { }
    }
}
