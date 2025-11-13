namespace TestOrderService.Application.Exceptions
{
    /// <summary>
    ///     Application exception custom.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class ApplicationException : Exception
    {

        /// <summary>
        ///     Initializes a new instance of the <see cref="ApplicationException" /> class.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        protected ApplicationException(string title, string message) : base(message)
        {
            Title = title;
        }
        /// <summary>
        ///     Gets the title.
        /// </summary>
        /// <value>
        ///     The title.
        /// </value>
        public string Title { get; }
    }
}
