using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.DTOs
{
    public class UserDto
    {
        public string Id { get; set; } = null!;
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string[] Roles { get; set; } = System.Array.Empty<string>();
    }
}
