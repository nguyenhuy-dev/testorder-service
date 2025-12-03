using MediatR;
using TestOrderService.Application.DTOs;
namespace TestOrderService.Application.Features.TestOrders.Queries.GetTestOrdersByPatient
{
    /// <summary>
    ///     Query to get all test orders for a specific patient
    /// </summary>
    public record GetTestOrdersByPatientQuery(Guid PatientId) : IRequest<List<TestOrderDto>>;
}
