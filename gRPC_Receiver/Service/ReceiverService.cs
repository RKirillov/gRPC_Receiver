using AutoMapper;
using Grpc.Core;
using Grpc.Net.Client;
using gRPC_Receiver.Entity;
using GrpcServices;
using System.Threading.Channels;

namespace gRPC_Receiver.Service
{
    public class ReceiverService : IReceiverService
    {
        private readonly ILogger<ReceiverService> _logger;
        private readonly SenderService.SenderServiceClient _senderServiceClient;
        private readonly IMapper _mapper;
        private readonly Channel<AdkuEntity> _channel;
        private int entityCounter = 0;
        public ReceiverService(ILogger<ReceiverService> logger, IMapper mapper)
        {
            _logger = logger;
            _mapper = mapper;

            // Создание ограниченного канала с емкостью 10000
            _channel = Channel.CreateBounded<AdkuEntity>(new BoundedChannelOptions(30000)
            {
                FullMode = BoundedChannelFullMode.Wait, // Блокирует запись, если канал переполнен
                SingleReader = true, // Разрешает только один поток для чтения
                SingleWriter = false  // Разрешает только один поток для записи
            });

            // Подключение к серверу gRPC (порт 5000, например)
            var grpcServerAddress = "http://localhost:5000"; // Адрес сервера gRPC
            var grpcChannel = GrpcChannel.ForAddress(grpcServerAddress);

            _senderServiceClient = new SenderService.SenderServiceClient(grpcChannel);
        }

        public async Task ReceiveEntitiesAsync(CancellationToken cancellationToken)
        {
            var request = new ReceiverRequest { RequestId = Guid.NewGuid().ToString() };
            using var call = _senderServiceClient.StreamEntities(request);

            try
            {
                await foreach (var grpcEntity in call.ResponseStream.ReadAllAsync(cancellationToken))
                {
                    var adkuEntity = _mapper.Map<AdkuEntity>(grpcEntity);
                    await AddToChannelAsync(adkuEntity, cancellationToken);
                    ;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении данных от SenderService");
            }
        }

        private async Task AddToChannelAsync(AdkuEntity adkuEntity, CancellationToken cancellationToken)
        {
            // Пытаемся записать в канал
            var written = await _channel.Writer.WaitToWriteAsync(cancellationToken); // Ждем, пока канал не станет доступен для записи

            if (written)
            {
                // Записываем в канал
                if (!_channel.Writer.TryWrite(adkuEntity))
                {
                    // В случае неудачи пробуем снова
                    await _channel.Writer.WaitToWriteAsync(cancellationToken);
                    _channel.Writer.TryWrite(adkuEntity);
                }
                entityCounter++;
                Console.WriteLine("{0}, counter: {1}", adkuEntity.Value.ToString(), entityCounter.ToString());

            }
        }

        public Channel<AdkuEntity> GetChannel()
        {
            return _channel;
        }
    }
}
