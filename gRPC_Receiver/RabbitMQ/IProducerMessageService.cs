
namespace gRPC_Receiver.RabbitMQ
{
    public interface IProducerMessageService : IDisposable
    {
        event Action<bool>? OnConnectionStatusChanged;

        void PublishMessage<T>(T message);
    }
}
