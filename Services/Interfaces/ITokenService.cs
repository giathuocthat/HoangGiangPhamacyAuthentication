using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface ITokenService
    {
        Task<(string AccessToken, string RefreshToken, DateTime ExpiresUtc)> GenerateTokenAsync(ApplicationUser user);
    }
}
