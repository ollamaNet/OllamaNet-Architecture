using System.Threading.Tasks;

namespace AuthService.DataSeeding.Interfaces
{
    public interface IDataSeeder
    {
        Task SeedAsync();
        Task<bool> ShouldSeedAsync();
    }
} 