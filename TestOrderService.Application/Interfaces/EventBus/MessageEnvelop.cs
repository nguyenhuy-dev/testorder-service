namespace TestOrderService.Application.Interfaces.EventBus
{
    public class MessageEnvelop
    {

        public MessageEnvelop() { }

        public MessageEnvelop(Type type, string message) : this(type.FullName!, message) { }

        public MessageEnvelop(string messageTypeName, string message)
        {
            MessageTypeName = messageTypeName ?? throw new ArgumentNullException(nameof(messageTypeName));
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }
        public string MessageTypeName { get; set; } = null!;

        public string Message { get; set; } = null!;
    }
}
