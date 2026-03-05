using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace SandwicheriaWalterio.Api.Services
{
    public interface IJwtService
    {
        string GenerarToken(int usuarioId, string nombreUsuario, string rol, string tenantId);
    }

    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerarToken(int usuarioId, string nombreUsuario, string rol, string tenantId)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "SandwicheriaWalterio_SuperSecretKey_2026_MuySegura!"));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()),
                new Claim(ClaimTypes.Name, nombreUsuario),
                new Claim(ClaimTypes.Role, rol),
                new Claim("TenantId", tenantId)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "SandwicheriaWalterio.Api",
                audience: _configuration["Jwt:Audience"] ?? "SandwicheriaWalterio.Client",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(12),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
