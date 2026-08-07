using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using StarterApp.Database.Models;

namespace StarterApp.Services;

// API authentication service using HTTP.
// When MauiProgram.cs has useSharedApi = true this class is used.
public class ApiAuthenticationService : IAuthenticationService
{
    // uses dependency injection from MauiProgram.cs to inject the httpClient singleton.
    // sets base address as API URL, so calls only require relative path.
    private readonly HttpClient _httpClient;

    // checks current session and their roles. Can be null.
    private User? _currentUser;
    private readonly List<string> _currentUserRoles = new();

    public event EventHandler<bool>? AuthenticationStateChanged;

    public bool IsAuthenticated => _currentUser != null;

    public User? CurrentUser => _currentUser;
    public List<string> CurrentUserRoles => _currentUserRoles;

    // sets JSON parser to ignore case sensitivity to stop parse values returning null.
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true    
    };

    // constructor for the dependency injection.
    public ApiAuthenticationService (HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // --- Login/Register/Logout functions --- //

    // attempts to login via email and password sent to API.
    public async Task<AuthToken> LoginAsync(string email, string password)
    {
        try
        {
            // sends email/password as an anonymous object.
            // PostAsJsonAsync converts anonymous object to JSON string.
            // sends POST request to API URL and waits for a response message.
            var request = new { email, password };
            var response = await _httpClient.PostAsJsonAsync("auth/token", request);

            // if response message is failure, parses JSON response body into TokenResponse.
            if (!response.IsSuccessStatusCode)
            {
                return AuthToken.Failure("Invalid email or password");
            }
            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(JsonOptions);

            // if unexpected response, send fail.
            if (tokenResponse == null)
            {
                return AuthToken.Failure("Login failed: unexpected server response");    
            }

            // save token to device secure storage.
            await SecureStorage.SetAsync("auth_token", tokenResponse.Token);

            // sets user_id as the userId from token response.
            await SecureStorage.SetAsync("user_id", tokenResponse.UserId.ToString());

            // sets token_expires_at time at Zulu time recorded on token response.
            await SecureStorage.SetAsync("token_expires_at", tokenResponse.ExpiresAt.ToString("o"));

            // use bearer token when API request is made
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenResponse.Token);

            // makes a HTTP GET request to /users/me.
            // hold the server reply and check if status code is success/failure.
            var profileResponse = await _httpClient.GetAsync("users/me");
            if (profileResponse.IsSuccessStatusCode)
            {
                // takes the API response and maps it to user model.
                // keeps password and hash empty as not expected.
                var profile = await profileResponse.Content
                    .ReadFromJsonAsync<UserProfileResponse>(JsonOptions);
                if (profile != null)
                {
                    _currentUser = new User
                    {
                        Id = profile.Id,
                        Email = profile.Email,
                        FirstName = profile.FirstName,
                        LastName = profile.LastName,
                        CreatedAt = profile.CreatedAt,
                        UpdatedAt = profile.CreatedAt,
                        IsActive = true,
                        PasswordHash = string.Empty,
                        PasswordSalt = string.Empty
                    };
                }
            }

            // sends auth=true state change.
            AuthenticationStateChanged?.Invoke(this, true);

            // return the AuthToken with fields filled.
            return AuthToken.Success(
                token: tokenResponse.Token,
                expiresAt: tokenResponse.ExpiresAt,
                userId: tokenResponse.UserId
            );
        }
        catch (Exception ex)
        {
            // catches unexpected failures.
            return AuthToken.Failure($"Login failed: {ex.Message}");
        }
    }

    public async Task<AuthToken> RegisterAsync(string firstName, string lastName, string email, string password)
    {
        try
        {
            // sends new anonymous object as JSON string via HTTP POST to API.
            // waits for a response.
            var request = new
            {
                firstName,
                lastName,
                email,
                password
            };
            var response = await _httpClient.PostAsJsonAsync("auth/register", request);

            // if response is failure code return reason to user.
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                return AuthToken.Failure(error?.Message ?? "Registration failed.");
            }

            // log user in after successful creation.
            return await LoginAsync(email, password);
        }
        catch (Exception ex)
        {
            // catches unexpected failures.
            return AuthToken.Failure($"Registration failed: {ex.Message}");
        }
    }

    // logout function clears session user and roles.
    // removes tokens from secure storage.
    // returns auth=false.
    public async Task LogoutAsync()
    {
        // clearing user session and roles.
        _currentUser = null;
        _currentUserRoles.Clear();

        // removing bearer token so authentication is rescinded.
        _httpClient.DefaultRequestHeaders.Authorization = null;

        // removing tokens from secure storage.
        SecureStorage.Remove("auth_token");
        SecureStorage.Remove("user_id");

        AuthenticationStateChanged?.Invoke(this, false);
        await Task.CompletedTask;
    }

    // checks if the token is expired.
    public async Task<bool> IsTokenExpiredAsync()
    {
        // gets the token expiry time from secure storage.
        var expiresAt = await SecureStorage.GetAsync("token_expires_at");

        // if expires at does not exist, set expired = true.
        if (string.IsNullOrEmpty(expiresAt)) return true;

        // return when the token expires.
        return DateTime.UtcNow >= DateTime.Parse(expiresAt);    
    }

    public async Task<bool> TryRestoreSessionAsync()
    {
        var token = await SecureStorage.GetAsync("auth_token");
        if (string.IsNullOrEmpty(token)) return false;
        if (await IsTokenExpiredAsync()) return false;

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var profileResponse = await _httpClient.GetAsync("users/me");
        if (!profileResponse.IsSuccessStatusCode) return false;

        var profile = await profileResponse.Content.ReadFromJsonAsync<UserProfileResponse>(JsonOptions);
        if (profile == null) return false;

        _currentUser = new User
        {
            Id = profile.Id, Email = profile.Email,
            FirstName = profile.FirstName, LastName = profile.LastName,
            CreatedAt = profile.CreatedAt, UpdatedAt = profile.CreatedAt,
            IsActive = true, PasswordHash = string.Empty, PasswordSalt = string.Empty
        };

        AuthenticationStateChanged?.Invoke(this, true);
        return true;
    }

    // --- Role Checks --- //

    // checks for a role via string comparison.
    public bool HasRole(string roleName) =>
        _currentUserRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase);

    // checks for any roles in a list.
    public bool HasAnyRole(params string[] roleNames) =>
        roleNames.Any(HasRole);

    // checks for all roles in a list
    public bool HasAllRoles(params string[] roleNames) =>
        roleNames.All(HasRole);
    
    // --- Password update --- //
    public Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        // Not supported by the shared API
        return Task.FromResult(false);
    }

    // --- API response DTOs --- //
    // describes JSON shapes the API send back.
    // lets ReadFromJsonAsync parse them properly.
    // private as rest of app only sees AuthToken/User.
    
    private record TokenResponse(string Token, DateTime ExpiresAt, int UserId);

    private record UserProfileResponse(
        int Id, string Email, string FirstName, string LastName, DateTime CreatedAt);
    
    private record ApiErrorResponse(string Error, string Message);
}