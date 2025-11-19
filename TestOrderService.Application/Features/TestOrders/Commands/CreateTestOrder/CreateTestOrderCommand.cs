using TestOrderService.Application.Interfaces.Message;
using TestOrderService.Domain.Entities;
namespace TestOrderService.Application.Features.TestOrders.Commands.CreateTestOrder
{
    /// <summary>
    ///     Create test order command.
    /// </summary>
    /// <seealso
    ///     cref="ICommand{Domain}.Entities.TestOrder&gt;" />
    public class CreateTestOrderCommand : ICommand<TestOrder>
    {
        /// <summary>
        ///     Gets or sets the test order identifier.
        /// </summary>
        /// <value>
        ///     The test order identifier.
        /// </value>
        public Guid TestOrderId { get; set; } = Guid.NewGuid();

        /// <summary>
        ///     Gets or sets the patient identifier.
        /// </summary>
        /// <value>
        ///     The patient identifier.
        /// </value>
        public Guid PatientId { get; set; }

        /// <summary>
        ///     Gets or sets the status.
        /// </summary>
        /// <value>
        ///     The status.
        /// </value>
        public StatusTestOrder Status { get; set; } = StatusTestOrder.Pending;

        /// <summary>
        ///     Gets or sets the create by identifier.
        /// </summary>
        /// <value>
        ///     The create by identifier.
        /// </value>
        public Guid CreateById { get; set; }

        /// <summary>
        ///     Gets or sets the create at.
        /// </summary>
        /// <value>
        ///     The create at.
        /// </value>
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        ///     Gets or sets the test order patient description.
        /// </summary>
        /// <value>
        ///     The test order patient description.
        /// </value>
        public string? TestOrderDescription { get; set; }
    }
}
