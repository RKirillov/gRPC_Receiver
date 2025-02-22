using gRPC_Receiver.Entity;

namespace gRPC_Receiver.Service
{
    public interface IChannelService
    {
        Task WriteAsync(AdkuEntity entity, CancellationToken cancellationToken);
        Task CompleteAsync();
    }

}
