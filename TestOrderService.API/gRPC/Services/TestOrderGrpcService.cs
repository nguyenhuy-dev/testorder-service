using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Mapster;
using MediatR;
using TestOrderService.API.gRPC.Protos;
using TestOrderService.Application.Features.TestOrders.Queries.GetAllTestOrders;
using TestOrderService.Application.Features.TestOrders.Queries.GetTestOrderById;
using Empty=TestOrderService.API.gRPC.Protos.Empty;

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

            foreach (var testOrder in testOrders)
            {
                var entity = new TestOrderEntity
                {
                    TestOrderId = testOrder.TestOrderId.ToString(),
                    PatientId = testOrder.PatientId.ToString(),
                    Status = (int)testOrder.Status,
                    ReviewId = testOrder.ReviewId?.ToString(),
                    ReviewAt = testOrder.ReviewAt.HasValue
                        ? Timestamp.FromDateTime(testOrder.ReviewAt.Value.ToUniversalTime())
                        : null,
                    CreateById = testOrder.CreateById.ToString(),
                    CreateAt = Timestamp.FromDateTime(testOrder.CreateAt.ToUniversalTime()),
                    RunById = testOrder.RunById.ToString(),
                    UpdateById = testOrder.UpdateById?.ToString(),
                    UpdateAt = testOrder.UpdateAt.HasValue
                        ? Timestamp.FromDateTime(testOrder.UpdateAt.Value.ToUniversalTime())
                        : null,
                    TestOrderDescription = testOrder.TestOrderDescription
                };

                response.TestOrders.Add(entity);
            }

            return response;
        }

        public override async Task<TestOrderEntity> GetTestOrderById(TestOrderRequest request, ServerCallContext context)
        {
            var query = new GetTestOrderByIdQuery(Guid.Parse(request.TestOrderId));

            var testOrder = await _sender.Send(query);

            var testOrderResponse = MapToTestOrderResponse(testOrder);

            return testOrderResponse;
        }

        private static TestOrderEntity MapToTestOrderResponse(Domain.Entities.TestOrder testOrder)
        {
            return new TestOrderEntity
            {
                TestOrderId = testOrder.TestOrderId.ToString(),
                PatientId = testOrder.PatientId.ToString(),
                Status = (int)testOrder.Status,
                ReviewId = testOrder.ReviewId.ToString(),
                ReviewAt = testOrder.ReviewAt.HasValue ? Timestamp.FromDateTime(testOrder.ReviewAt.Value) : null,
                CreateById = testOrder.CreateById.ToString(),
                CreateAt = Timestamp.FromDateTime(testOrder.CreateAt),
                UpdateById = testOrder.UpdateById.ToString(),
                UpdateAt = testOrder.UpdateAt.HasValue ? Timestamp.FromDateTime(testOrder.UpdateAt.Value) : null,
                RunById = testOrder.RunById.ToString(),
                RunAt = testOrder.RunAt.HasValue ? Timestamp.FromDateTime(testOrder.RunAt.Value) : null,
                TestOrderDescription = testOrder.TestOrderDescription
            };
        }
    }
}
