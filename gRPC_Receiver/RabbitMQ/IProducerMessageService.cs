

namespace gRPC_Receiver.RabbitMQ
{
    public interface IProducerMessageService 
    {
        Task PublishMessage<T>(T message);
    }
}
