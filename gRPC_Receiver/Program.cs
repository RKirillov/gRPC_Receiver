
using GrpcServices;
using gRPC_Receiver.Service;
using gRPC_Receiver.Mapper;
using gRPC_Receiver.Interseptors;
using gRPC_Receiver.JWT; // Пространство имен для вашего gRPC сервиса
using gRPC_Receiver.RabbitMQ;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Threading.Channels;
using gRPC_Receiver.Entity;

// Регистрация сервисов
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IReceiverService, ReceiverService>();
// Регистрация самого сервиса, который использует IConnectionFactory
builder.Services.AddSingleton<IProducerMessageService, ProducerMessageService>();
builder.Services.AddSingleton<IChannelService, ChannelService>();
builder.Services.AddAutoMapper(typeof(EntityMappingProfile));
builder.Services.AddHostedService<ReceiverServiceWithTimer>(); // Регистрация фонового сервиса
builder.Services.AddSingleton<ITokenProvider, AppTokenProvider>();
builder.Services.AddSingleton<ClientLoggingInterceptor>();

// Регистрация канала как singleton
var channel = Channel.CreateBounded<AdkuEntity>(new BoundedChannelOptions(300000)
{
    FullMode = BoundedChannelFullMode.Wait,
    SingleReader = true,
    SingleWriter = false

});
builder.Services.AddSingleton(channel);

// Регистрация фоновой службы
builder.Services.AddHostedService<ChannelProcessingService>();

builder.Services.AddGrpcClient<SenderService.SenderServiceClient>(options =>
{
    options.Address = new Uri("https://localhost:5000"); // Укажите адрес вашего gRPC-сервера
})
        //Bearer token with gRPC client factory
        .AddCallCredentials((context, metadata, serviceProvider) =>
        {
            // Получаем сервис из DI контейнера для получения токена
            var provider = serviceProvider.GetRequiredService<ITokenProvider>();
            var _token =  provider.GetToken(context.CancellationToken);
            if (!string.IsNullOrEmpty(_token))
            {
                metadata.Add("Authorization", $"Bearer {_token}");
            }
            return Task.CompletedTask;
        })
.AddInterceptor<ClientLoggingInterceptor>(); // Добавляем интерсептор;


// Регистрация настроек из appsettings.json
builder.Services.Configure<BusConnectOptions>(builder.Configuration.GetSection("BusConnectOptions"));


// Регистрация фабрики соединений как Singleton
builder.Services.AddSingleton<IConnectionFactory>(provider =>
{
    var busOptions = provider.GetRequiredService<IOptions<BusConnectOptions>>().Value;
    return new ConnectionFactory()
    {
        UserName = busOptions.Username,
        Password = busOptions.Password,
        HostName = busOptions.Host,
        Port = busOptions.Port,
        VirtualHost = busOptions.VirtualHost
    };
});




// Настройка Swagger (если используется)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Конфигурация HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();


