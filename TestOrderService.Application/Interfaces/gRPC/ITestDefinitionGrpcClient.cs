using TestOrderService.Application.DTOs.gRPC.GetAllTestDefinitions;
namespace TestOrderService.Application.Interfaces.gRPC
{
    public interface ITestDefinitionGrpcClient
    {
        Task<List<TestDefinitionDto>> GetAllTestDefinitions(CancellationToken cancellation);
    }
}
