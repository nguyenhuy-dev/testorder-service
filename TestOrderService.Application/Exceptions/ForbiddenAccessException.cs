namespace TestOrderService.Application.Exceptions
{
    /// <summary>
    ///     Forbidden access exception custom.
    /// </summary>
    /// <seealso cref="TestOrderService.Application.Exceptions.ApplicationException" />
    public class ForbiddenAccessException : ApplicationException
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ForbiddenAccessException" /> class.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        public ForbiddenAccessException(string title, string message) : base(title, message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ForbiddenAccessException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ForbiddenAccessException(string message) : base("Forbidden Access", message) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ForbiddenAccessException" /> class.
        /// </summary>
        public ForbiddenAccessException() : base("Forbidden Access", "User does not have permission to access this resource.") { }
    }
}
