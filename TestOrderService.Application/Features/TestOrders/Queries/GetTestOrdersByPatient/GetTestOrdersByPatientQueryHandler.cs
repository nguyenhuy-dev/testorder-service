using MediatR;
using TestOrderService.Application.DTOs;
using TestOrderService.Application.Interfaces;
namespace TestOrderService.Application.Features.TestOrders.Queries.GetTestOrdersByPatient
{
    public class GetTestOrdersByPatientQueryHandler : IRequestHandler<GetTestOrdersByPatientQuery, List<TestOrderDto>>
    {
        private readonly ITestOrderRepository _testOrderRepository;

        public GetTestOrdersByPatientQueryHandler(ITestOrderRepository testOrderRepository)
        {
            _testOrderRepository = testOrderRepository;
        }

        /// <summary>
        ///     Handles the GetTestOrdersByPatientQuery request
        /// </summary>
        /// <param name="request">Query containing patient ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of test orders for the patient</returns>
        public async Task<List<TestOrderDto>> Handle(GetTestOrdersByPatientQuery request, CancellationToken cancellationToken)
        {
            return await _testOrderRepository.GetTestOrdersByPatientIdAsync(request.PatientId, cancellationToken);
        }
    }
}
