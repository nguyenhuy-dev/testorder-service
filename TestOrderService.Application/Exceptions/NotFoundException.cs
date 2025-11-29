namespace TestOrderService.Application.Exceptions
{
    /// <summary>
    ///     Not found exception custom.
    /// </summary>
    /// <seealso cref="TestOrderService.Application.Exceptions.ApplicationException" />
    public class NotFoundException : ApplicationException
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NotFoundException" /> class.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        public NotFoundException(string title, string message) : base(title, message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NotFoundException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public NotFoundException(string message) : base("Not Found", message) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NotFoundException" /> class.
        /// </summary>
        /// <param name="nameValue">The name value.</param>
        /// <param name="key">The key.</param>
        public NotFoundException(string key, object value) : base("Not Found", $"{key} {value} was not found.") { }
    }
}
