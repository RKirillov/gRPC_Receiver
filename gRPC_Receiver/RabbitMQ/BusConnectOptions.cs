namespace gRPC_Receiver.RabbitMQ
{
    public class BusConnectOptions
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string VirtualHost { get; set; }
        public int DeliveryMode { get; set; }
        public string Expiration { get; set; } // Время жизни в миллисекундах
        public string ContentType { get; set; }
    }

}
