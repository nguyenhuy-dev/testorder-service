using MonitoringService.API.gRPC.Protos.TestDefinitionProto;
using TestOrderService.Application.DTOs.gRPC.GetAllTestDefinitions;
using TestOrderService.Application.Interfaces.gRPC;

namespace TestOrderService.Infrastructure.gRPC.Clients
{
    public class TestDefinitionGrpcClient : ITestDefinitionGrpcClient
    {
        private readonly TestDefinition.TestDefinitionClient _client;

        public TestDefinitionGrpcClient(TestDefinition.TestDefinitionClient client)
        {
            _client = client;
        }

        public async Task<List<TestDefinitionDto>> GetAllTestDefinitions(CancellationToken cancellation)
        {
            var response = await _client.GetAllTestDefinitionsAsync(
                new Empty(),
                cancellationToken: cancellation
            );

            return response.TestDefinitions.Select(t => new TestDefinitionDto
            {
                TestDefinitionId = t.TestDefinitionId,
                TestName = t.TestName,
                TestCode = t.TestCode,
                Unit = t.Unit
            }).ToList();
        }
    }
}
