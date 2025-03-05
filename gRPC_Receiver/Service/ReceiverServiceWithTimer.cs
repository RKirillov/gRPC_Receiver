namespace gRPC_Receiver.Service
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using gRPC_Receiver.RabbitMQ;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public class ReceiverServiceWithTimer : BackgroundService
    {
        private readonly ILogger<ReceiverServiceWithTimer> _logger;
        private readonly IReceiverService _receiverService;
        private Timer _timer;
        private bool _isProcessing = false;

        public ReceiverServiceWithTimer(ILogger<ReceiverServiceWithTimer> logger, IReceiverService receiverService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _receiverService = receiverService ?? throw new ArgumentNullException(nameof(receiverService));
            _timer = new Timer(ReceiveEntitiesCallback, null, Timeout.Infinite, Timeout.Infinite);
            // Подписка на событие изменения статуса подключения
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Запуск таймера, который будет вызывать метод каждые 5 секунд
            _timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(5));
            return Task.CompletedTask;
        }

        private async void ReceiveEntitiesCallback(object? state)
        {
            if (_isProcessing)
                return;

            try
            {
                _isProcessing = true;
                await _receiverService.ReceiveEntitiesAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении данных с gRPC сервера.");
            }
            finally
            {
                _isProcessing = false;
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            // Остановка таймера при завершении работы приложения
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            return base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            // Освобождение ресурсов
            _timer?.Dispose();
            base.Dispose();
        }
    }
}
