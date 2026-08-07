using Microsoft.EntityFrameworkCore;
using StarterApp.Database.Data;
using StarterApp.Database.Models;
using BCrypt.Net;

namespace StarterApp.Services;

// Uses the postgresql db for authentication. Offline only use.
// When MauiProgram.cs has useSharedApi = false this class is used.
public class LocalAuthenticationService : IAuthenticationService
{
    private readonly AppDbContext _context;
    private User? _currentUser;
    private List<string> _currentUserRoles = new();

    public event EventHandler<bool>? AuthenticationStateChanged;

    public LocalAuthenticationService(AppDbContext context)
    {
        _context = context;
    }

    public bool IsAuthenticated => _currentUser != null;

    public User? CurrentUser => _currentUser;

    public List<string> CurrentUserRoles => _currentUserRoles;

    // --- Login/Registration/Logout --- //

    // attempts to use email/password given against db entry.
    public async Task<AuthToken> LoginAsync(string email, string password)
    {
        // try/catch exception block for error handling.
        try
        {
            // if account is active, look up email.
            // include user role in query.
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

            // if no user found, fail login.
            if (user == null)
            {
                return AuthToken.Failure("Invalid email or password");
            }

            // BCrypt.Verify checks against stored hash for password.
            // If password incorrect, fail login.
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return AuthToken.Failure("Invalid email or password");
            }

            // login success. Stores user details and roles for session.
            _currentUser = user;
            _currentUserRoles = user.UserRoles
                .Where(ur => ur.IsActive)
                .Select(ur => ur.Role.Name)
                .ToList();

            // sends bool=true state change to IAuthenticationService(AuthenticationStateChanged).
            AuthenticationStateChanged?.Invoke(this, true);

            // passes user details to AuthToken.
            return AuthToken.Success(userId: user.Id);
        }
        catch (Exception ex)
        {
            // catches exception and reports login failed with message.
            return AuthToken.Failure($"Login failed: {ex.Message}");
        }
    }

    // new user registration function for database.
    public async Task<AuthToken> RegisterAsync(string firstName, string lastName, string email, string password)
    {
        try
        {
            // Check if user already exists.
            // fails if email already in db.
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
            {
                return AuthToken.Failure("User with this email already exists");
            }

            // Create password hash.
            var salt = BCrypt.Net.BCrypt.GenerateSalt();
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, salt);

            // Create new user.
            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PasswordHash = hashedPassword,
                PasswordSalt = salt,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // add new user to db and save changes.
            // need to save now so user.Id is generated before roles can be assigned.
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Assign default "User" role.
            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.IsDefault == true);
            if (userRole != null)
            {
                var userRoleAssignment = new UserRole(user.Id, userRole.Id);
                _context.UserRoles.Add(userRoleAssignment);
                await _context.SaveChangesAsync();
            }

            // returns auth token for successful registration.
            return AuthToken.Success(userId: user.Id);
        }
        catch (Exception ex)
        {
            // returns fail for registration attempt.
            return AuthToken.Failure($"Registration failed: {ex.Message}");
        }
    }

    // clears all session information and sends state=false to log out.
    public Task LogoutAsync()
    {
        _currentUser = null;
        _currentUserRoles.Clear();
        AuthenticationStateChanged?.Invoke(this, false);
        return Task.CompletedTask;
    }

    public Task<bool> TryRestoreSessionAsync() => Task.FromResult(false);
    // --- Role Checks --- //

    // checking for specific role using string comparison.
    public bool HasRole(string roleName)
    {
        return _currentUserRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
    }

    // checks for any role within a list, returns true if found.
    public bool HasAnyRole(params string[] roleNames)
    {
        return roleNames.Any(role => HasRole(role));
    }

    // checks for all roles within a list, returns false if any not found.
    public bool HasAllRoles(params string[] roleNames)
    {
        return roleNames.All(role => HasRole(role));
    }


    // --- New Password --- //
    public async Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        // if no session, return false.
        if (_currentUser == null)
            return false;

        try
        {
            // if password hash does not match, fail attempt.
            if (!BCrypt.Net.BCrypt.Verify(currentPassword, _currentUser.PasswordHash))
            {
                return false;
            }

            // create new hashed password.
            var salt = BCrypt.Net.BCrypt.GenerateSalt();
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword, salt);

            // assign new password to session and save changes.
            _currentUser.PasswordHash = hashedPassword;
            _currentUser.PasswordSalt = salt;
            _currentUser.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(_currentUser);
            await _context.SaveChangesAsync();

            return true;
        }
        catch
        {
            // catches unexpected exceptions.
            return false;
        }
    }
}