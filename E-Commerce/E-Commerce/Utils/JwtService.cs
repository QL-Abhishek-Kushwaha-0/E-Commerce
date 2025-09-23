using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using E_Commerce.Models.Entities;
using Microsoft.IdentityModel.Tokens;

namespace E_Commerce.Utils
{
    public interface IJwtService
    {
        string GenerateJwtToken(User user, string tokenType);
    }
    public class JwtService:IJwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }
        public string GenerateJwtToken(User user, string tokenType)
        {
            if (user == null) return "Invalid User!!";

            var jwtSettings = _config.GetSection("JwtSettings");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));     // Secret Key
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Role, user.Role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: tokenType.Equals("Access", StringComparison.OrdinalIgnoreCase) ? DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["AccessTokenExpiryMinutes"])) : DateTime.UtcNow.AddDays(double.Parse(jwtSettings["RefreshTokenExpiryDays"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
