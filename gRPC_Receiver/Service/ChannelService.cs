using gRPC_Receiver.Entity;
using System.Threading.Channels;

namespace gRPC_Receiver.Service
{
    public class ChannelService : IChannelService
    {
        private readonly Channel<AdkuEntity> _channel;

        public ChannelService()
        {
            _channel = Channel.CreateBounded<AdkuEntity>(10000);
        }

        public async Task WriteAsync(AdkuEntity entity, CancellationToken cancellationToken)
        {
            await _channel.Writer.WriteAsync(entity, cancellationToken);
        }

        public Task CompleteAsync()
        {
            _channel.Writer.Complete();
            return Task.CompletedTask;
        }

        public ChannelReader<AdkuEntity> GetReader() => _channel.Reader;
    }

}
