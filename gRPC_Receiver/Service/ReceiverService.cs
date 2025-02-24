using AutoMapper;
using Grpc.Core;
using gRPC_Receiver.Entity;
using GrpcServices;
using System;
using System.Threading.Channels;

namespace gRPC_Receiver.Service
{
    public class ReceiverService : IReceiverService
    {
        private readonly ILogger<ReceiverService> _logger;
/*        In your ReceiverService class, the StreamEntities method is invoked on _senderServiceClient, which is an instance of SenderService.SenderServiceClient.This indicates that ReceiverService is acting as a client in this context, making requests to a gRPC server.*/
        private readonly SenderService.SenderServiceClient _senderServiceClient;
        private readonly IMapper _mapper;
        private readonly Channel<AdkuEntity> _channel;
        private int entityCounter = 0;
        public ReceiverService(ILogger<ReceiverService> logger, IMapper mapper, SenderService.SenderServiceClient senderServiceClient)
        {
            _logger = logger;
            _mapper = mapper;

            // Создание ограниченного канала с емкостью 10000
            _channel = System.Threading.Channels.Channel.CreateBounded<AdkuEntity>(new BoundedChannelOptions(30000)
            {
                FullMode = BoundedChannelFullMode.Wait, // Блокирует запись, если канал переполнен
                SingleReader = true, // Разрешает только один поток для чтения
                SingleWriter = false  // Разрешает только один поток для записи
            });

            // Подключение к серверу gRPC (порт 5000, например)
            /*            var grpcServerAddress = "http://localhost:5000"; // Адрес сервера gRPC
                        var grpcChannel = GrpcChannel.ForAddress(grpcServerAddress);

                        _senderServiceClient = new SenderService.SenderServiceClient(grpcChannel);*/
            _senderServiceClient = senderServiceClient;
        }

        public async Task ReceiveEntitiesAsync(CancellationToken cancellationToken)
        {
            var request = new ReceiverRequest { RequestId = Guid.NewGuid().ToString() };
            //Represents an asynchronous server-streaming call that returns multiple GrpcServices.Entity messages.
            using AsyncServerStreamingCall<GrpcServices.Entity>? call = _senderServiceClient.StreamEntities(request);

            try
            {
                //call.ResponseStream: An asynchronous stream of GrpcServices.Entity messages sent by the server.
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
                Console.WriteLine("value {0}, datetime: {1}, datetimeUTC: {2}, RegisterType: {3}", adkuEntity.Value.ToString(), adkuEntity.DateTime.ToString(), adkuEntity.DateTimeUTC.ToString(),adkuEntity.RegisterType);

            }
        }

        public Channel<AdkuEntity> GetChannel()
        {
            return _channel;
        }
    }
}
