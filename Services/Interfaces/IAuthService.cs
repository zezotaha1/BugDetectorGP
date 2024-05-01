using BugDetectorGP.Dto;
using BugDetectorGP.Models;

namespace BugDetectorGP.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthModel> RegisterAsync(RegisterUser model);
        Task<AuthModel> LoginAsync(LoginUser model);
        Task<AuthModel> RefreshTokenAsync(string Token);
        Task<bool> LogoutAsync(string token);

    }
}
