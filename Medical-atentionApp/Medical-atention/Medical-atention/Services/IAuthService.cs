using Medical_atention.Models;
using System.Threading.Tasks;

namespace Medical_atention.Services
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(string email, string password);

        // Returns true on success, false on 401 (wrong current password), throws on network error
        Task<bool> ChangePasswordAsync(string token, string currentPassword, string newPassword);
    }
}
