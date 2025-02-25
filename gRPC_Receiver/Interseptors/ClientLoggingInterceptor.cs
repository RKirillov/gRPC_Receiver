
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace gRPC_Receiver.Interseptors
{
    public class ClientLoggingInterceptor : Interceptor
    {
        private readonly ILogger<ClientLoggingInterceptor> _logger;

        public ClientLoggingInterceptor(ILogger<ClientLoggingInterceptor> logger)
        {
            _logger = logger;
        }

        // Override the AsyncUnaryCall method to intercept unary calls
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            _logger.LogInformation("Starting call. Type/Method: {Type} / {Method}",
                context.Method.Type, context.Method.Name);
            return continuation(request, context);
        }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            _logger.LogInformation("Starting call. Type/Method: {Type} / {Method}",
                context.Method.Type, context.Method.Name);
            return continuation(request, context);
        }


        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            _logger.LogInformation("Starting call. Type/Method: {Type} / {Method}",
                context.Method.Type, context.Method.Name);
            return continuation(context);
        }
    }
}
