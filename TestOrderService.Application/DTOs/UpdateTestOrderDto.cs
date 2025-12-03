using TestOrderService.Domain.Entities;

namespace TestOrderService.Application.DTOs
{
    /// <summary>
    ///     Update test order data transfer object.
    ///     Backend only accepts description and status from client now.
    /// </summary>
    public sealed record UpdateTestOrderDto(
        string? TestOrderDescription,
        StatusTestOrder? Status
    );
}
