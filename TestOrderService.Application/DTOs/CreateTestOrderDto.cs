namespace TestOrderService.Application.DTOs
{
    /// <summary>
    ///     Create test order data tranfer object.
    /// </summary>
    /// <seealso cref="System.IEquatable&lt;TestOrderService.Application.DTOs.CreateTestOrderDto&gt;" />
    public sealed record CreateTestOrderDto(string? TestOrderDescription);
}
