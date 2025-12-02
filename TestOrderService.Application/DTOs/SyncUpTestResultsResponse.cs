using TestOrderService.Domain.Entities;
namespace TestOrderService.Application.DTOs
{
    public class SyncUpTestResultsResponse(List<TestOrder> testOrders)
    {
        public int Count { get; private set; } = testOrders.Count;

        public List<TestOrder> TestOrders { get; private set; } = testOrders;

        public void UpdateTestOrders(List<TestOrder> updatedTestOrders)
        {
            TestOrders = updatedTestOrders;
            Count = updatedTestOrders.Count;
        }
    }
}
