using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using LMS.Infrastructure.Data;
using LMS.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LMS.Infrastructure.Auth;

public sealed class AuthService : IAuthService
{
    private const int MaxFailedAttempts = 3;
    private const int RefreshTokenLifetimeDays = 7;

    private readonly LmsDbContext _db;
    private readonly IJwtService _jwt;
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;

    public AuthService(
        LmsDbContext db,
        IJwtService jwt,
        IConfiguration config,
        HttpClient httpClient
    )
    {
        _db = db;
        _jwt = jwt;
        _config = config;
        _httpClient = httpClient;
    }

    public string GetSsoAuthorizationUrl(string? state = null)
    {
        var tenantId = RequireConfig("AzureAd__TenantId");
        var clientId = RequireConfig("AzureAd__ClientId");
        var redirectUri = RequireConfig("AzureAd__RedirectUri");

        var query = new StringBuilder();
        query.Append("response_type=code");
        query.Append("&client_id=").Append(Uri.EscapeDataString(clientId));
        query.Append("&redirect_uri=").Append(Uri.EscapeDataString(redirectUri));
        query.Append("&scope=").Append(Uri.EscapeDataString("openid profile email"));
        if (!string.IsNullOrWhiteSpace(state))
        {
            query.Append("&state=").Append(Uri.EscapeDataString(state));
        }

        return $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize?{query}";
    }

    public async Task<AuthResult?> HandleSsoCallbackAsync(
        string code,
        string? state,
        string ipAddress,
        CancellationToken ct = default
    )
    {
        var tenantId = RequireConfig("AzureAd__TenantId");
        var clientId = RequireConfig("AzureAd__ClientId");
        var clientSecret = RequireConfig("AzureAd__ClientSecret");
        var redirectUri = RequireConfig("AzureAd__RedirectUri");

        var tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
        var formContent = new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["redirect_uri"] = redirectUri,
            }
        );

        var response = await _httpClient.PostAsync(tokenEndpoint, formContent, ct);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<AzureTokenResponse>(
            cancellationToken: ct
        );
        if (tokenResponse is null || string.IsNullOrEmpty(tokenResponse.IdToken))
        {
            return null;
        }

        var email = ExtractEmailFromIdToken(tokenResponse.IdToken);
        if (email is null)
        {
            return null;
        }

        var user = await _db
            .Users.Where(u => u.Email == email && u.DeletedAt == null)
            .FirstOrDefaultAsync(ct);

        if (user is null || user.Status != "Active")
        {
            return null;
        }

        return await IssueTokensAsync(user, ipAddress, ct);
    }

    public async Task<AuthResult?> LoginAsync(
        string email,
        string password,
        string ipAddress,
        CancellationToken ct = default
    )
    {
        var user = await _db
            .Users.Where(u => u.Email == email && u.DeletedAt == null)
            .FirstOrDefaultAsync(ct);

        if (user is null || user.PasswordHash is null)
        {
            return null;
        }

        if (user.Status == "Locked" || user.Status == "Inactive")
        {
            return null;
        }

        var passwordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        if (!passwordValid)
        {
            user.FailedAttempts++;
            if (user.FailedAttempts >= MaxFailedAttempts)
            {
                user.Status = "Locked";
                user.LockedAt = DateTime.UtcNow;
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return null;
        }

        user.FailedAttempts = 0;
        user.UpdatedAt = DateTime.UtcNow;
        return await IssueTokensAsync(user, ipAddress, ct);
    }

    public async Task<AuthResult?> RefreshAsync(
        string rawRefreshToken,
        string ipAddress,
        CancellationToken ct = default
    )
    {
        var tokenHash = JwtService.HashRefreshToken(rawRefreshToken);
        var stored = await _db
            .RefreshTokens.Include(rt => rt.User)
            .Where(rt => rt.TokenHash == tokenHash)
            .FirstOrDefaultAsync(ct);

        if (stored is null || stored.RevokedAt is not null || stored.ExpiresAt <= DateTime.UtcNow)
        {
            return null;
        }

        var user = stored.User;
        if (user.DeletedAt is not null || user.Status != "Active")
        {
            return null;
        }

        stored.RevokedAt = DateTime.UtcNow;
        stored.UpdatedAt = DateTime.UtcNow;
        return await IssueTokensAsync(user, ipAddress, ct);
    }

    public async Task LogoutAsync(string rawRefreshToken, CancellationToken ct = default)
    {
        var tokenHash = JwtService.HashRefreshToken(rawRefreshToken);
        var stored = await _db
            .RefreshTokens.Where(rt => rt.TokenHash == tokenHash)
            .FirstOrDefaultAsync(ct);

        if (stored is null)
        {
            return;
        }

        stored.RevokedAt = DateTime.UtcNow;
        stored.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    private async Task<AuthResult> IssueTokensAsync(
        User user,
        string ipAddress,
        CancellationToken ct
    )
    {
        var accessToken = await _jwt.GenerateAccessTokenAsync(user, ct);
        var rawRefreshToken = _jwt.GenerateRawRefreshToken();
        var tokenHash = JwtService.HashRefreshToken(rawRefreshToken);

        _db.RefreshTokens.Add(
            new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenLifetimeDays),
                IpAddress = ipAddress,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            }
        );

        await _db.SaveChangesAsync(ct);

        return new AuthResult
        {
            AccessToken = accessToken,
            RawRefreshToken = rawRefreshToken,
            ExpiresInSeconds = 86400,
        };
    }

    private string RequireConfig(string key) =>
        _config[key]
        ?? throw new InvalidOperationException($"Configuration key '{key}' is not set.");

    private static string? ExtractEmailFromIdToken(string idToken)
    {
        var parts = idToken.Split('.');
        if (parts.Length < 2)
        {
            return null;
        }

        var payload = parts[1];
        payload = payload.Replace('-', '+').Replace('_', '/');
        var padding = payload.Length % 4;
        if (padding > 0)
        {
            payload += new string('=', 4 - padding);
        }

        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("email", out var emailProp))
            {
                return emailProp.GetString();
            }

            if (doc.RootElement.TryGetProperty("preferred_username", out var upnProp))
            {
                return upnProp.GetString();
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private sealed class AzureTokenResponse
    {
        [JsonPropertyName("id_token")]
        public string? IdToken { get; init; }

        [JsonPropertyName("access_token")]
        public string? AccessToken { get; init; }
    }
}
