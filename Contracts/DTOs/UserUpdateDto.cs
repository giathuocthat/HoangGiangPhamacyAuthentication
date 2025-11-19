using System.ComponentModel.DataAnnotations;

namespace Contracts.DTOs
{
    public class UserUpdateDto
    {
        [StringLength(100)]
        public string? Username { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(200)]
        public string? FullName { get; set; }
    }
}
