using Contracts.DTOs;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HoangGiangPhamacyAuthentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // POST: api/user
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = new ApplicationUser
            {
                UserName = dto.Username,
                Email = dto.Email,
                FullName = dto.FullName
            };

            var result = await _userService.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
            }

            // return location to the created resource
            var response = new { id = user.Id, username = user.UserName, email = user.Email };
            return Created($"/api/user/{user.Id}", response);
        }

        // PUT: api/user/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UserUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = await _userService.GetByIdAsync(id);
            if (existing == null) return NotFound(new { error = "User not found." });

            // Map updatable fields
            existing.FullName = dto.FullName ?? existing.FullName;
            existing.Email = dto.Email ?? existing.Email;
            existing.UserName = dto.Username ?? existing.UserName;

            var result = await _userService.UpdateAsync(existing);
            if (!result.Succeeded)
            {
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
            }

            return Ok(new { id = existing.Id, username = existing.UserName, email = existing.Email, fullName = existing.FullName });
        }

        // GET: api/user/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { error = "id_required" });

            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound(new { error = "user_not_found" });

            var roles = await _userService.GetRolesAsync(user);

            var dto = new UserDto
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                Roles = roles?.ToArray() ?? Array.Empty<string>()
            };

            return Ok(dto);
        }
    }
}
