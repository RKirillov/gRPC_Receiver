using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text.Json;
using System.Text;

namespace gRPC_Receiver.RabbitMQ
{
    public class ProducerMessageService : IProducerMessageService
    {
        private const string Exchange = "Pcf.ReceivingFromPartner.Promocodes";
        private const string RoutingKey = "Pcf.ReceivingFromPartner.Promocode";

        private readonly IConnectionFactory _connectionFactory;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public ProducerMessageService(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
            _connection = _connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public  Task PublishMessage<T>(T message)
        {
            var body = JsonSerializer.Serialize(message);
            var bytes = Encoding.UTF8.GetBytes(body);

            _channel.BasicPublish(
                exchange: Exchange,
                routingKey: RoutingKey,
                body: bytes);

            return Task.CompletedTask;
        }

        // Метод для закрытия соединения и канала при завершении работы приложения

/*        В ASP.NET Core контейнер зависимостей автоматически вызывает метод Dispose для сервисов, зарегистрированных с временем жизни Scoped или Transient, при завершении запроса.Однако для сервисов с временем жизни Singleton метод Dispose вызывается при завершении работы приложения.*/ 

/*        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }*/
    }
}
