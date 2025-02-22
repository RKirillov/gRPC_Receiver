using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;

namespace gRPC_Receiver.Interseptors
{
    public class ClientLoggingInterceptor : Interceptor
    {
        private readonly ILogger<ClientLoggingInterceptor> _logger;

        public ClientLoggingInterceptor(ILogger<ClientLoggingInterceptor> logger)
        {
            _logger = logger;
        }

        // Обработка Unary запросов (например, для метода StreamEntities)
        public override async Task<TResponse> UnaryClientHandler<TRequest, TResponse>(
            TRequest request,
            ClientCallContext context,
            UnaryClientMethod<TRequest, TResponse> continuation)
        {
            _logger.LogInformation("Sending Unary request: {Method} with request: {Request}", context.Method, request);

            // Выполнение запроса
            var response = await continuation(request, context);

            _logger.LogInformation("Received Unary response: {Method} with response: {Response}", context.Method, response);

            return response;
        }
    }

}
