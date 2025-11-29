using TestOrderService.Application.Interfaces;
using TestOrderService.Domain.Entities;
using TestOrderService.Infrastructure.Data;
namespace TestOrderService.Infrastructure.Repositories
{
    public class TestResultRepository(TestOrderServiceDbContext dbContext) : ITestResultRepository
    {
        private readonly TestOrderServiceDbContext _dbContext = dbContext;

        public async Task<TestResult> CreateTestResult(TestResult testResult, CancellationToken cancellationToken)
        {
            await _dbContext.AddAsync(testResult, cancellationToken);

            return testResult;
        }
    }
}
