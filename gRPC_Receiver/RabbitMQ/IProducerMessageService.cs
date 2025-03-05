
namespace gRPC_Receiver.RabbitMQ
{
    public interface IProducerMessageService : IDisposable
    {
        void PublishMessage<T>(T message);
    }
}
