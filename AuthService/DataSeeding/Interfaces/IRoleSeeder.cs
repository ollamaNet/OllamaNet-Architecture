using System.Threading.Tasks;
using System.Collections.Generic;

namespace AuthService.DataSeeding.Interfaces
{
    public interface IRoleSeeder
    {
        Task SeedRolesAsync(IEnumerable<string> roles);
    }
} 