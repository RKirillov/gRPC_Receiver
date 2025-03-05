using gRPC_Receiver.Entity;
using System.Threading.Channels;

namespace gRPC_Receiver.RabbitMQ
{
    public class ChannelProcessingService : BackgroundService
    {
        private readonly Channel<AdkuEntity> _channel;
        private readonly IProducerMessageService _producerMessageService;
        private readonly ILogger<ChannelProcessingService> _logger;

        public ChannelProcessingService(
            Channel<AdkuEntity> channel,
            IProducerMessageService producerMessageService,
            ILogger<ChannelProcessingService> logger)
        {
            _channel = channel;
            _producerMessageService = producerMessageService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await foreach (var entity in _channel.Reader.ReadAllAsync(stoppingToken))
                {
                    try
                    {
                        _producerMessageService.PublishMessage(entity);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ошибка при отправке сообщения в RabbitMQ");
                        await Task.Delay(10000);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Логирование отмены операции, если необходимо
                _logger.LogInformation("Обработка канала была отменена.");
            }
            catch (Exception ex)
            {
                // Логирование других возможных ошибок
                _logger.LogError(ex, "Произошла ошибка при чтении из канала.");
            }
        }
    }

}
