namespace CRM.Application.Services.Auth_Service;

public interface IAuthService
{
    Task<LoginResponseVM> LoginAsync(LoginRequestVM model);
    Task<bool> RemoveRefreshTokenAsync(LogOutRequestVM model);
    Task<LoginResponseVM> RefreshTokenAsync(LogOutRequestVM model);
}
