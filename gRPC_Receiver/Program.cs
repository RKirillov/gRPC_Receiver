using gRPC_Receiver.Interseptors;
using gRPC_Receiver.Mapper;
using gRPC_Receiver.Service;
namespace gRPC_Receiver
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);



            builder.Services.AddSingleton<IReceiverService, ReceiverService>();
            builder.Services.AddSingleton<IChannelService, ChannelService>();
            builder.Services.AddAutoMapper(typeof(EntityMappingProfile));
            builder.Services.AddHostedService<ReceiverServiceWithTimer>();  // Регистрация фонового сервиса

            builder.Services.AddSingleton<LoggingInterceptor>();
            builder.Services.AddGrpc(options =>
            {
                options.Interceptors.Add<LoggingInterceptor>();
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.Run();
        }
    }
}