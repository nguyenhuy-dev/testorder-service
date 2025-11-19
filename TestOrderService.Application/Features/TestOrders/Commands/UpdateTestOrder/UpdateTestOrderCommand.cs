using TestOrderService.Application.Interfaces.Message;
using TestOrderService.Domain.Entities;
namespace TestOrderService.Application.Features.TestOrders.Commands.UpdateTestOrder
{
    /// <summary>
    ///     Update test order command.
    /// </summary>
    /// <seealso cref="ICommand{TestOrder}" />
    public class UpdateTestOrderCommand : ICommand<TestOrder>
    {
        /// <summary>
        ///     Gets or sets the test order identifier.
        /// </summary>
        /// <value>
        ///     The test order identifier.
        /// </value>
        public Guid TestOrderId { get; set; }

        /// <summary>
        ///     Gets or sets the run by identifier.
        /// </summary>
        /// <value>
        ///     The run by identifier.
        /// </value>
        public Guid? RunById { get; set; }

        /// <summary>
        ///     Gets or sets the run at.
        /// </summary>
        /// <value>
        ///     The run at.
        /// </value>
        public DateTime? RunAt { get; set; }

        /// <summary>
        ///     Gets or sets the test order patient description.
        /// </summary>
        /// <value>
        ///     The test order patient description.
        /// </value>
        public string? TestOrderDescription { get; set; }

        /// <summary>
        ///     Gets or sets the status.
        /// </summary>
        /// <value>
        ///     The status.
        /// </value>
        public StatusTestOrder? Status { get; set; }

        /// <summary>
        ///     Gets or sets the update by identifier.
        /// </summary>
        /// <value>
        ///     The update by identifier.
        /// </value>
        public Guid UpdateById { get; set; }

        /// <summary>
        ///     Gets or sets the update at.
        /// </summary>
        /// <value>
        ///     The update at.
        /// </value>
        public DateTime UpdateAt { get; set; } = DateTime.UtcNow;
    }
}
