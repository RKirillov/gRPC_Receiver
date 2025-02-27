
namespace gRPC_Receiver.RabbitMQ
{
    public interface IProducerMessageService : IDisposable
    {
        Task PublishMessage<T>(T message);
    }
}
