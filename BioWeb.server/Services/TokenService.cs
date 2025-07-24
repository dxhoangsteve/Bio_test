using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BioWeb.Server.Services
{
    public interface ITokenService
    {
        string GenerateToken(string username);
        bool ValidateToken(string token);
        ClaimsPrincipal? GetPrincipalFromToken(string token);
    }

    public class TokenService : ITokenService
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;

        public TokenService(IConfiguration configuration)
        {
            _secretKey = configuration["Jwt:SecretKey"] ?? "BioWeb-Super-Secret-Key-For-JWT-Token-Generation-2024";
            _issuer = configuration["Jwt:Issuer"] ?? "BioWeb.Server";
            _audience = configuration["Jwt:Audience"] ?? "BioWeb.Client";
        }

        public string GenerateToken(string username)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim("username", username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24), // Token hết hạn sau 24h
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool ValidateToken(string token)
        {
            try
            {
                var principal = GetPrincipalFromToken(token);
                return principal != null;
            }
            catch
            {
                return false;
            }
        }

        public ClaimsPrincipal? GetPrincipalFromToken(string token)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
                var tokenHandler = new JwtSecurityTokenHandler();

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
