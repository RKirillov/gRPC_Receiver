namespace gRPC_Receiver.JWT
{
    public interface ITokenProvider
    {
        string GetToken(CancellationToken cancellationToken);
    }

    public class AppTokenProvider : ITokenProvider
    {
        private string? _token;

        public string GetToken(CancellationToken cancellationToken)
        {
            if (_token == null)
            {
                // App code to resolve the token here.
                _token = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InRlc3QxIiwibmJmIjoxNTgxOTYyNzI0LCJleHAiOjE1ODE5NjYzMjQsImlhdCI6MTU4MTk2MjcyNH0.VvYln0PgZQrFwBTx0Ik3TGGI43DxdVVxzHAXma-K5P0";
            }

            return _token;
        }
    }
}
