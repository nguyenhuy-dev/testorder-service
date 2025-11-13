namespace TestOrderService.Application.Exceptions
{
    /// <summary>
    ///     Bad request exception custom.
    /// </summary>
    /// <seealso cref="TestOrderService.Application.Exceptions.ApplicationException" />
    public class BadRequestException : ApplicationException
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BadRequestException" /> class.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        public BadRequestException(string title, string message) : base(title, message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BadRequestException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public BadRequestException(string message) : base("Bad Request", message) { }
    }
}
