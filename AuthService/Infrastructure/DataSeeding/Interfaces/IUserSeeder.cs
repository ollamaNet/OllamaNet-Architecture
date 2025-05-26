using System.Threading.Tasks;
using AuthService.DataSeeding.Options;

namespace AuthService.DataSeeding.Interfaces
{
    public interface IUserSeeder
    {
        Task SeedAdminUserAsync(AdminAccountOptions options);
    }
} 