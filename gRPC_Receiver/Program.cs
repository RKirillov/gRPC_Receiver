using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Grpc.Net.Client;
using GrpcServices;
using gRPC_Receiver.Service;
using gRPC_Receiver.Mapper;
using gRPC_Receiver.RabbitMQ;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using gRPC_Receiver.Interseptors; // Пространство имен для вашего gRPC сервиса

// Регистрация сервисов
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IReceiverService, ReceiverService>();
builder.Services.AddSingleton<IChannelService, ChannelService>();
builder.Services.AddAutoMapper(typeof(EntityMappingProfile));
builder.Services.AddHostedService<ReceiverServiceWithTimer>(); // Регистрация фонового сервиса

builder.Services.AddSingleton<ClientLoggingInterceptor>();

builder.Services.AddGrpcClient<SenderService.SenderServiceClient>(options =>
{
    options.Address = new Uri("http://localhost:5000"); // Укажите адрес вашего gRPC-сервера
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

// Регистрация самого сервиса, который использует IConnectionFactory
builder.Services.AddScoped<IProducerMessageService, ProducerMessageService>();


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
