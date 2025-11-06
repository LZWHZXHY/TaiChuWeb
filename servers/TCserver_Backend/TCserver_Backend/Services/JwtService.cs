using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Hosting;

namespace TCserver_Backend.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public JwtService(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        public string GenerateAccessToken(IEnumerable<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 根据环境设置Issuer和Audience
            var issuer = _env.IsDevelopment()
                ? _configuration["Jwt:Issuer:Development"]
                : _configuration["Jwt:Issuer:Production"];

            var audience = _env.IsDevelopment()
                ? _configuration["Jwt:Audience:Development"]
                : _configuration["Jwt:Audience:Production"];

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:AccessTokenExpiry"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString();
        }

        public ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _env.IsDevelopment()
                    ? _configuration["Jwt:Issuer:Development"]
                    : _configuration["Jwt:Issuer:Production"],
                ValidateAudience = true,
                ValidAudience = _env.IsDevelopment()
                    ? _configuration["Jwt:Audience:Development"]
                    : _configuration["Jwt:Audience:Production"],
                ValidateLifetime = false // 不验证过期时间，因为我们要刷新
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out var securityToken);

                if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid token");
                }

                return principal;
            }
            catch (Exception ex)
            {
                throw new SecurityTokenException("Token validation failed", ex);
            }
        }
    }
}