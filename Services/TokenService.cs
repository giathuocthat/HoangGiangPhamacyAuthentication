using Domain.Entities;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IRepository<ApplicationUser> _userRepository;

        public TokenService(UserManager<ApplicationUser> userManager, IConfiguration configuration,
            IRepository<ApplicationUser> userRepository)
        {
            _userManager = userManager;
            _configuration = configuration;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Generates a signed JWT access token and a random refresh token.
        /// Returns (AccessToken, RefreshToken, ExpiresUtc).
        /// Requires Jwt:Key, Jwt:Issuer, Jwt:Audience, Jwt:ExpiresMinutes in configuration.
        /// </summary>
        public async Task<(string AccessToken, string RefreshToken, DateTime ExpiresUtc)> GenerateTokenAsync(ApplicationUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var key = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"] ?? "your_issuer";
            var audience = _configuration["Jwt:Audience"] ?? "your_audience";
            var expiresMinutes = int.TryParse(_configuration["Jwt:ExpiresMinutes"], out var m) ? m : 60;

            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException("JWT signing key is not configured (Jwt:Key).");

            // Base claims
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
            };

            if (!string.IsNullOrEmpty(user.Email))
                claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));

            if (!string.IsNullOrEmpty(user.FullName))
                claims.Add(new Claim("fullName", user.FullName));

            // Include claims from UserManager
            var userClaims = await _userManager.GetClaimsAsync(user);
            if (userClaims != null)
                claims.AddRange(userClaims);

            // Include roles as claims
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var keyBytes = Encoding.UTF8.GetBytes(key);
            var signingKey = new SymmetricSecurityKey(keyBytes);
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddMinutes(expiresMinutes);

            var jwt = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expires,
                signingCredentials: signingCredentials
            );

            var handler = new JwtSecurityTokenHandler();
            var accessToken = handler.WriteToken(jwt);

            var refreshToken = GenerateRefreshToken();

            return (AccessToken: accessToken, RefreshToken: refreshToken, ExpiresUtc: expires);
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
