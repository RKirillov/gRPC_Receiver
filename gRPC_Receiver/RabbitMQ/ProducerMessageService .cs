using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using MassTransit;
using Polly;
using Polly.Retry;

namespace gRPC_Receiver.RabbitMQ
{
    public class ProducerMessageService : IProducerMessageService
    {

        private readonly IBus _bus;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly ILogger<ProducerMessageService> _logger;
        public ProducerMessageService( IBus bus, ILogger<ProducerMessageService> logger)
        {
            _bus = bus;
            _logger = logger;
            // 🔹 Создаем политику повторных попыток (Polly)
            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Экспоненциальная задержка
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning($"Попытка {retryCount} отправки сообщения не удалась. Повтор через {timeSpan.TotalSeconds} сек. Ошибка: {exception.Message}");
                    });
        }


        public async Task PublishMessage<T>(T message)
        {
            // 🔹 Проверка на null, чтобы избежать исключений
            ArgumentNullException.ThrowIfNull(message, nameof(message));

            var endpoint = await _bus.GetSendEndpoint(new Uri($"queue:MyQueue"));

            // 🔹 Отправляем сообщение с `RetryPolicy`
            await _retryPolicy.ExecuteAsync(async () =>
            {
                await endpoint.Send(message);
                _logger.LogInformation($"✅ Сообщение отправлено успешно: {typeof(T).Name}");
            });
        }
        // Метод для закрытия соединения и канала при завершении работы приложения

        /*        В ASP.NET Core контейнер зависимостей автоматически вызывает метод Dispose для сервисов, зарегистрированных с временем жизни Scoped или Transient, при завершении запроса.Однако для сервисов с временем жизни Singleton метод Dispose вызывается при завершении работы приложения.*/
    }
}
