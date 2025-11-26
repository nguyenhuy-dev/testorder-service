using TestOrderService.Domain.Entities;
namespace TestOrderService.Application.Interfaces
{
    public interface ITestResultRepository
    {
        Task<TestResult> CreateTestResult(TestResult testResult, CancellationToken cancellationToken);
    }
}
