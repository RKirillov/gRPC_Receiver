
using GrpcServices;
using gRPC_Receiver.Service;
using gRPC_Receiver.Mapper;
using gRPC_Receiver.Interseptors;
using gRPC_Receiver.JWT; // ������������ ���� ��� ������ gRPC �������
using gRPC_Receiver.RabbitMQ;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Threading.Channels;
using gRPC_Receiver.Entity;

// ����������� ��������
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IReceiverService, ReceiverService>();
// ����������� ������ �������, ������� ���������� IConnectionFactory
builder.Services.AddSingleton<IProducerMessageService, ProducerMessageService>();
builder.Services.AddSingleton<IChannelService, ChannelService>();
builder.Services.AddAutoMapper(typeof(EntityMappingProfile));
builder.Services.AddHostedService<ReceiverServiceWithTimer>(); // ����������� �������� �������
builder.Services.AddSingleton<ITokenProvider, AppTokenProvider>();
builder.Services.AddSingleton<ClientLoggingInterceptor>();

// ����������� ������ ��� singleton
var channel = Channel.CreateBounded<AdkuEntity>(new BoundedChannelOptions(300000)
{
    FullMode = BoundedChannelFullMode.Wait,
    SingleReader = true,
    SingleWriter = false

});
builder.Services.AddSingleton(channel);

// ����������� ������� ������
builder.Services.AddHostedService<ChannelProcessingService>();

builder.Services.AddGrpcClient<SenderService.SenderServiceClient>(options =>
{
    options.Address = new Uri("https://localhost:5000"); // ������� ����� ������ gRPC-�������
})
        //Bearer token with gRPC client factory
        .AddCallCredentials((context, metadata, serviceProvider) =>
        {
            // �������� ������ �� DI ���������� ��� ��������� ������
            var provider = serviceProvider.GetRequiredService<ITokenProvider>();
            var _token =  provider.GetToken(context.CancellationToken);
            if (!string.IsNullOrEmpty(_token))
            {
                metadata.Add("Authorization", $"Bearer {_token}");
            }
            return Task.CompletedTask;
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


