using TestOrderService.Domain.Entities;
namespace TestOrderService.Application.DTOs
{
    /// <summary>
    ///     Update test order data transfer object.
    /// </summary>
    public sealed record UpdateTestOrderDto(
        Guid? RunById,
        DateTime? RunAt,
        string? TestOrderDescription,
        StatusTestOrder? Status,
        string? AIReviewSummary
    );
}
