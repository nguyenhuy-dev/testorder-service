using Microsoft.EntityFrameworkCore;
using TestOrderService.Application.Interfaces;
using TestOrderService.Domain.Entities;
namespace TestOrderService.Infrastructure.Repositories
{
    public class TestResultRepository(DbContext dbContext) : ITestResultRepository
    {
        private readonly DbContext _dbContext = dbContext;

        public async Task<TestResult> CreateTestResult(TestResult testResult, CancellationToken cancellationToken)
        {
            await _dbContext.AddAsync(testResult, cancellationToken);

            return testResult;
        }
    }
}
