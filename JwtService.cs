using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

public class JwtService
{
    private readonly string _secretKey;

    public JwtService(string secretKey)
    {
        _secretKey = secretKey;
    }

    public string GenerateJwtToken(string userId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Convert.FromBase64String(_secretKey);
        var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId)
        };

        var token = new JwtSecurityToken(
            issuer: "TRPO",
            audience: "chats",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return tokenHandler.WriteToken(token);
    }
    public ClaimsPrincipal ValidateJwtToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Convert.FromBase64String(_secretKey);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "TRPO",
            ValidAudience = "chats",
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };

        try
        {
            SecurityToken validatedToken;
            var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
            return principal;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
}