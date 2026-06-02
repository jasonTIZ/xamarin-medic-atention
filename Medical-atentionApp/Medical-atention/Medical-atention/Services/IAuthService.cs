using Medical_atention.Models;
using System.Threading.Tasks;

namespace Medical_atention.Services
{
    public interface IAuthService
    {
        // Returns LoginResponse on success, null on 401, throws on network error
        Task<LoginResponse> LoginAsync(string email, string password);
    }
}
