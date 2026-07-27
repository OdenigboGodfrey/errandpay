using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TransferPlatform.Api.Authentication;
using TransferPlatform.src.TransferPlatform.Api.DTOs;

namespace TransferPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly JwtSettings _jwtSettings;

    public AuthController(
        IConfiguration configuration,
        IOptions<JwtSettings> jwtOptions)
    {
        _configuration = configuration;
        _jwtSettings = jwtOptions.Value;
    }

    [HttpPost("login")]
    public IActionResult Login(
        [FromBody] LoginRequest request)
    {
        var configuredUsername =
            _configuration["Auth:Username"];

        var configuredPassword =
            _configuration["Auth:Password"];

        if (request.Username != configuredUsername ||
            request.Password != configuredPassword)
        {
            return ResponseHelper.BuildResponse<LoginResponse>("401", false, "Invalid username or password", null);
        }

        var tokenHandler = new JwtSecurityTokenHandler();

        var key = Encoding.UTF8.GetBytes(
            _jwtSettings.SecretKey);

        var expiresAt =
            DateTime.UtcNow.AddMinutes(
                _jwtSettings.ExpiryMinutes);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, request.Username),
                new Claim(ClaimTypes.Role, "Admin")
            }),

            Expires = expiresAt,

            Issuer = _jwtSettings.Issuer,

            Audience = _jwtSettings.Audience,

            SigningCredentials =
                new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
        };

        var token =
            tokenHandler.CreateToken(tokenDescriptor);

        var jwt =
            tokenHandler.WriteToken(token);


        return ResponseHelper.BuildResponse<LoginResponse>("200", true, "Login successful", new LoginResponse
        {
            Token = jwt,
            ExpiresAt = expiresAt
        });
    }
}