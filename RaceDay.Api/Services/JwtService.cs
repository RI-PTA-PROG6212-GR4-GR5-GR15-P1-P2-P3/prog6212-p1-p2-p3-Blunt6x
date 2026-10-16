using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RaceDay.Api.Models;
namespace RaceDay.Api.Services;
public class JwtService(IConfiguration config) {
    public string CreateToken(UserAccount user) {
        var key = config["Jwt:Key"] ?? "DEV_ONLY_CHANGE_THIS_KEY_TO_A_LONG_RANDOM_SECRET_2026";
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()), new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"), new Claim(ClaimTypes.Email, user.Email), new Claim(ClaimTypes.Role, user.Role) };
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
        var minutes = int.TryParse(config["Jwt:ExpiresInMinutes"], out var m) ? m : 60;
        var token = new JwtSecurityToken(config["Jwt:Issuer"] ?? "RaceDayApi", config["Jwt:Audience"] ?? "RaceDayClients", claims, expires: DateTime.UtcNow.AddMinutes(minutes), signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
