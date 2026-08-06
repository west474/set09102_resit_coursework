using StarterApp.Database.Models;

namespace StarterApp.Services;

// LocalAuthenticationService and ApiAuthenticationService rely on this.
// Obfuscates them from the rest of the app via dependency injection.
public interface IAuthenticationService
{
    // --- Current session functions --- //

    // passes bool=true for login and bool=false for logout.
    event EventHandler<bool>? AuthenticationStateChanged;
    
    // checks if someone is logged in and their details/roles. Null if false.
    bool IsAuthenticated { get; }
    User? CurrentUser { get; }
    List<string> CurrentUserRoles { get; }

    // --- Login/Registration Functions --- //

    // login attempt using email and password.
    // returns success/fail authentication token.
    Task<AuthToken> LoginAsync(string email, string password);

    // registration attempt using details from registration form.
    // returns success/fail authentication token.
    Task<AuthToken> RegisterAsync(string firstName, string lastName, string email, string password);
    Task LogoutAsync();
    
    // --- Role Checks --- //

    // looks for a specific role.
    bool HasRole(string roleName);

    // looks for at least one role in specified list.
    bool HasAnyRole(params string[] roleNames);

    // looks for all roles in specified list.
    bool HasAllRoles(params string[] roleNames);
    
    // change password function.
    Task<bool> ChangePasswordAsync(string currentPassword, string newPassword);
}