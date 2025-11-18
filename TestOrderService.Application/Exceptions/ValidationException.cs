namespace TestOrderService.Application.Exceptions
{
    /// <summary>
    ///     Validation exception custom.
    /// </summary>
    /// <seealso cref="TestOrderService.Application.Exceptions.ApplicationException" />
    public class ValidationException : ApplicationException
    {

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValidationException" /> class.
        /// </summary>
        /// <param name="errorsDictionary">The errors dictionary.</param>
        public ValidationException(IReadOnlyDictionary<string, string[]> errorsDictionary) : base("Validation Failure", "Validation failed.")
        {
            ErrorsDictionary = errorsDictionary;
        }

        /// <summary>
        ///     Gets or sets the errors dictionary.
        /// </summary>
        /// <value>
        ///     The errors dictionary.
        /// </value>
        public IReadOnlyDictionary<string, string[]> ErrorsDictionary { get; set; }
    }
}
