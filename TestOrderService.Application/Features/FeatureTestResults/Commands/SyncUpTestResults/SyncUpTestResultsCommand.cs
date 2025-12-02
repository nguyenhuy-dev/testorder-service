using TestOrderService.Application.Interfaces.Message;
using TestOrderService.Domain.Entities;
namespace TestOrderService.Application.Features.FeatureTestResults.Commands.SyncUpTestResults
{
    public sealed record SyncUpTestResultsCommand(Guid UpdateBy) : ICommand<List<TestOrder>>;
}
