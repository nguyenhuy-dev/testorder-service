namespace TestOrderService.Application.Exceptions
{
    /// <summary>
    ///     Exception for business rule violations.
    /// </summary>
    public class BusinessRuleException : ApplicationException
    {
        public BusinessRuleException()
            : base("Business rule violated", "A business rule has been violated.")
        {
        }

        public BusinessRuleException(string title, string message)
            : base(title, message)
        {
        }
    }
}
