using Contracts.DTOs;
using Contracts.Responses;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace HoangGiangPhamacyAuthentication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly TokenService _tokenService;
        private readonly UserManager<ApplicationUser> _userManager;

        public TokenController(TokenService tokenService, UserManager<ApplicationUser> userManager)
        {
            _tokenService = tokenService;
            _userManager = userManager;
        }

        // POST: api/token
        [HttpPost]
        public async Task<IActionResult> Token([FromBody] LoginDTO model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null) return Unauthorized(new { error = "invalid_credentials" });

            var valid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!valid) return Unauthorized(new { error = "invalid_credentials" });

            var (accessToken, refreshToken, expiresUtc) = await _tokenService.GenerateTokenAsync(user);

            var response = new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresUtc = expiresUtc,
                TokenType = "Bearer"
            };

            return Ok(response);
        }
    }

}