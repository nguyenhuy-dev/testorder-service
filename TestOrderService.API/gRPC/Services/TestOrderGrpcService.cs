using Grpc.Core;
using Mapster;
using MediatR;
using TestOrderService.API.gRPC.Protos;
using TestOrderService.Application.Features.TestOrders.Queries.GetAllTestOrders;
namespace TestOrderService.API.gRPC.Services
{
    public class TestOrderGrpcService(ISender sender) : TestOrder.TestOrderBase
    {
        private readonly ISender _sender = sender;

        public override async Task<TestOrderListResponse> GetAllTestOrders(Empty request, ServerCallContext context)
        {
            var query = new GetAllTestOrdersQuery();
            var testOrders = await _sender.Send(query);

            var response = new TestOrderListResponse();
            response.TestOrders.AddRange(testOrders.Select(t => t.Adapt<TestOrderEntity>()));

            return response;
        }
    }
}
