using System.ComponentModel.DataAnnotations;

namespace Contracts.DTOs
{
    public class UserCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Username { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = null!;

        [StringLength(200)]
        public string? FullName { get; set; }
    }
}
