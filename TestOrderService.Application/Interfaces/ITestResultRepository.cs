using TestOrderService.Domain.Entities;
namespace TestOrderService.Application.Interfaces
{
    public interface ITestResultRepository
    {
        Task<TestResult> CreateTestResultAsync(TestResult testResult, CancellationToken cancellationToken);
    }
}
