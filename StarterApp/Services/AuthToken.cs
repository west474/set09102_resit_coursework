namespace StarterApp.Services;

// AuthToken is an object returned by each login or registration attempt.
// Confirms whether the caller's attempt was a success or not.
// Carries the JWT token data if successful.
public class AuthToken
{
    // true/false if login/registration worked or not.
    public bool IsSuccess { get; set; }

    // human-readable message for success or failure.
    public string Message { get; set; } = string.Empty;

    // jwt string returned by the API.
    // needs to be nullable (?) because of LocalAuthenticationService not producing token.
    public string? Token { get; set; }

    // expiry date/time of token. Also nullable for LocalAuthenticationService.
    public DateTime? ExpiresAt { get; set; }

    // user ID for login, needed to associate with jwt.
    public int? UserId { get; set; }

    // --- Success/Fail Checks --- //
    
    // successful auth function.
    public static AuthToken Success(string? token = null, DateTime? expiresAt = null, int? userId = null)
    {
        // returning API received auth token.
        return new AuthToken
        {
            IsSuccess = true,
            Message = "Success",
            Token = token,
            ExpiresAt = expiresAt,
            UserId = userId
        };
    }

    // failed auth function.
    public static AuthToken Failure(string message)
    {
        // only returns message since no token to attach.
        return new AuthToken
        {
            IsSuccess = false,
            Message = message
        };
    }
}