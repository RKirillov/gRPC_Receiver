using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Grpc.Net.Client;
using GrpcServices;
using gRPC_Receiver.Service;
using gRPC_Receiver.Mapper;
using gRPC_Receiver.Interseptors;
using gRPC_Receiver.JWT; // ������������ ���� ��� ������ gRPC �������

// ����������� ��������
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IReceiverService, ReceiverService>();
builder.Services.AddSingleton<IChannelService, ChannelService>();
builder.Services.AddAutoMapper(typeof(EntityMappingProfile));
builder.Services.AddHostedService<ReceiverServiceWithTimer>(); // ����������� �������� �������
builder.Services.AddScoped<ITokenProvider, AppTokenProvider>();
builder.Services.AddSingleton<ClientLoggingInterceptor>();

builder.Services.AddGrpcClient<SenderService.SenderServiceClient>(options =>
{
    options.Address = new Uri("http://localhost:5000"); // ������� ����� ������ gRPC-�������
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


