using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Grpc.Net.Client;
using GrpcServices;
using gRPC_Receiver.Service;
using gRPC_Receiver.Mapper;
using gRPC_Receiver.RabbitMQ;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using gRPC_Receiver.Interseptors; // ������������ ���� ��� ������ gRPC �������

// ����������� ��������
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IReceiverService, ReceiverService>();
builder.Services.AddSingleton<IChannelService, ChannelService>();
builder.Services.AddAutoMapper(typeof(EntityMappingProfile));
builder.Services.AddHostedService<ReceiverServiceWithTimer>(); // ����������� �������� �������

builder.Services.AddSingleton<ClientLoggingInterceptor>();

builder.Services.AddGrpcClient<SenderService.SenderServiceClient>(options =>
{
    options.Address = new Uri("http://localhost:5000"); // ������� ����� ������ gRPC-�������
})
.AddInterceptor<ClientLoggingInterceptor>(); // ��������� �����������;


// ����������� �������� �� appsettings.json
builder.Services.Configure<BusConnectOptions>(builder.Configuration.GetSection("BusConnectOptions"));


// ����������� ������� ���������� ��� Singleton
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

// ����������� ������ �������, ������� ���������� IConnectionFactory
builder.Services.AddScoped<IProducerMessageService, ProducerMessageService>();


// ��������� Swagger (���� ������������)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ������������ HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();
