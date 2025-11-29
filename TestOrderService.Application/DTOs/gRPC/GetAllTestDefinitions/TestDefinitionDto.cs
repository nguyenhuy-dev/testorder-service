namespace TestOrderService.Application.DTOs.gRPC.GetAllTestDefinitions
{
    public class TestDefinitionDto
    {
        public int TestDefinitionId { get; set; }

        public string TestName { get; set; } = default!;

        public string TestCode { get; set; } = default!;

        public string Unit { get; set; } = default!;
    }
}
