namespace LMS.Infrastructure.Auth;

public interface IAuthService
{
    string GetSsoAuthorizationUrl(string? state = null);

    Task<AuthResult?> HandleSsoCallbackAsync(
        string code,
        string? state,
        string ipAddress,
        CancellationToken ct = default
    );

    Task<AuthResult?> LoginAsync(
        string email,
        string password,
        string ipAddress,
        CancellationToken ct = default
    );

    Task<AuthResult?> RefreshAsync(
        string rawRefreshToken,
        string ipAddress,
        CancellationToken ct = default
    );

    Task LogoutAsync(string rawRefreshToken, CancellationToken ct = default);
}

public sealed class AuthResult
{
    public string AccessToken { get; init; } = string.Empty;
    public string RawRefreshToken { get; init; } = string.Empty;
    public int ExpiresInSeconds { get; init; } = 86400;
}
