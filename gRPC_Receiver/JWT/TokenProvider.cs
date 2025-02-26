
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
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
                // Ваши настройки для создания токена
                var secretKey = "This is my test private keyThis is my test private key";  // Убедитесь, что длина ключа >= 32 байта;  // Пример секретного ключа
                var issuer = "YourIssuer";  // Укажите Issuer (например, ваш сервис)
                var audience = "YourAudience";  // Укажите Audience (например, целевая система)

                // Устанавливаем срок действия токена (например, 1 час)
                var expiration = DateTime.UtcNow.AddHours(1);

                // Создание объекта для создания токена
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                // Создание объекта JWT
                var jwtToken = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    expires: expiration,
                    signingCredentials: signingCredentials,
                    claims: new List<System.Security.Claims.Claim>
                    {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Admin")  // Добавьте нужные клеймы
                    }
                );

                // Создание JWT токена
                var tokenHandler = new JwtSecurityTokenHandler();
                _token = tokenHandler.WriteToken(jwtToken);  // Генерация токена в строковом формате
            }

            return _token;
        }
    }
}
