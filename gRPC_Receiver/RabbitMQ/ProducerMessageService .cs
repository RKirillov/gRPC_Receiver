using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text.Json;
using System.Text;
using Polly;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;

namespace gRPC_Receiver.RabbitMQ
{
    public class ProducerMessageService : IProducerMessageService
    {
        private const string Exchange = "Pcf.ReceivingFromPartner.Promocodes";
        private const string RoutingKey = "Pcf.ReceivingFromPartner.Promocode";

        private readonly IConnectionFactory _connectionFactory;
        private  IConnection? _connection;
        private  IModel? _channel;
        private readonly BusConnectOptions _busConnectOptions;

        public ProducerMessageService(IConnectionFactory connectionFactory, IOptions<BusConnectOptions> busConnectOptions)
        {
            _connectionFactory = connectionFactory;
            InitializeConnection();

            _busConnectOptions = busConnectOptions.Value;

        }
        private void InitializeConnection()
        {
            // Определяем политику повторных попыток с использованием Polly
            var retryPolicy = Policy
                .Handle<BrokerUnreachableException>() // Обрабатываем исключение BrokerUnreachableException
                .Or<SocketException>() // Также обрабатываем исключения сокетов
                .WaitAndRetryForeverAsync(retryAttempt =>
                {
                    // Вычисляем экспоненциальную задержку с максимальным значением 60 секунд
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                    var result= delay > TimeSpan.FromSeconds(60) ? TimeSpan.FromSeconds(60) : delay;
                    return result;
                },
                (exception, result, context) =>
                {
                    // Логируем информацию о повторной попытке
                    Console.WriteLine($"Попытка подключения не удалась. Повтор через {result.ToString()} секунд. Ошибка: {exception.Message}");
                });

            // Выполняем политику повторных попыток
            retryPolicy.ExecuteAsync(async () =>
            {
                _connection = _connectionFactory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.ExchangeDeclare(
                    exchange: Exchange,
                    type: ExchangeType.Direct,
                    durable: true,
                    autoDelete: false
                );
                await Task.CompletedTask;
            }).GetAwaiter().GetResult();

            if (_connection == null || !_connection.IsOpen)
            {
                throw new InvalidOperationException("Не удалось установить соединение с RabbitMQ.");
            }
        }

        public  Task PublishMessage<T>(T message)
        {
            if (_channel == null || !_channel.IsOpen)
            {
                throw new InvalidOperationException("Канал не инициализирован или закрыт.");
            }

            var body = JsonSerializer.Serialize(message);
            var bytes = Encoding.UTF8.GetBytes(body);
            var properties = _channel.CreateBasicProperties();

            properties.DeliveryMode = (byte)_busConnectOptions.DeliveryMode; // Установка DeliveryMode
            properties.Expiration = _busConnectOptions.Expiration; // Установка Expiration
            properties.ContentType = _busConnectOptions.ContentType; // Установка ContentType
            properties.Persistent = true;

            _channel.BasicPublish(
                exchange: Exchange,
                routingKey: RoutingKey,
                basicProperties: properties,
                body: bytes);

            return Task.CompletedTask;
        }

        // Метод для закрытия соединения и канала при завершении работы приложения

        /*        В ASP.NET Core контейнер зависимостей автоматически вызывает метод Dispose для сервисов, зарегистрированных с временем жизни Scoped или Transient, при завершении запроса.Однако для сервисов с временем жизни Singleton метод Dispose вызывается при завершении работы приложения.*/

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
