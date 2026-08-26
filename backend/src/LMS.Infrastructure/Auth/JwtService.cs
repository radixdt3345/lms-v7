using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LMS.Infrastructure.Data;
using LMS.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LMS.Infrastructure.Auth;

/// <summary>
/// RS256 JWT service. Uses the active Rs256Key from the database for signing.
/// The private key is stored AES-256-GCM encrypted at rest; the encryption key
/// comes from the JWT__KeyEncryptionKey environment variable (32 bytes, base64).
/// </summary>
public sealed class JwtService : IJwtService
{
    private const int AccessTokenLifetimeHours = 24;
    private const int RsaKeySize = 2048;

    private readonly LmsDbContext _db;
    private readonly IConfiguration _config;

    public JwtService(LmsDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<string> GenerateAccessTokenAsync(User user, CancellationToken ct = default)
    {
        var key = await GetActiveRsaKeyAsync(ct);

        using var rsa = RSA.Create();
        rsa.ImportRSAPrivateKey(DecryptPrivateKey(key.PrivateKeyEncrypted), out _);

        var signingKey = new RsaSecurityKey(rsa.ExportParameters(includePrivateParameters: true))
        {
            KeyId = key.Id.ToString(),
        };
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.RsaSha256);

        var issuer = _config["Jwt__Issuer"] ?? "lms-api";
        var audience = _config["Jwt__Audience"] ?? "lms-client";

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.Name),
            new Claim("role", user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(AccessTokenLifetimeHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRawRefreshToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    public async Task EnsureActiveKeyAsync(CancellationToken ct = default)
    {
        var exists = await _db.Rs256Keys.AnyAsync(k => k.IsActive, ct);
        if (!exists)
        {
            await GenerateAndPersistKeyAsync(ct);
        }
    }

    private async Task<Rs256Key> GetActiveRsaKeyAsync(CancellationToken ct)
    {
        var key = await _db.Rs256Keys.FirstOrDefaultAsync(k => k.IsActive, ct);
        if (key is null)
        {
            key = await GenerateAndPersistKeyAsync(ct);
        }

        return key;
    }

    private async Task<Rs256Key> GenerateAndPersistKeyAsync(CancellationToken ct)
    {
        using var rsa = RSA.Create(RsaKeySize);

        var publicKeyDer = rsa.ExportRSAPublicKey();
        var privateKeyDer = rsa.ExportRSAPrivateKey();

        var encryptedPrivate = EncryptPrivateKey(privateKeyDer);

        var entity = new Rs256Key
        {
            Id = Guid.NewGuid(),
            PublicKey = Convert.ToBase64String(publicKeyDer),
            PrivateKeyEncrypted = encryptedPrivate,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        _db.Rs256Keys.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    private string EncryptPrivateKey(byte[] privateKeyBytes)
    {
        var encKey = GetEncryptionKey();
        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize];
        RandomNumberGenerator.Fill(nonce);

        var ciphertext = new byte[privateKeyBytes.Length];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize];

        using var aes = new AesGcm(encKey, AesGcm.TagByteSizes.MaxSize);
        aes.Encrypt(nonce, privateKeyBytes, ciphertext, tag);

        var combined = new byte[nonce.Length + ciphertext.Length + tag.Length];
        Buffer.BlockCopy(nonce, 0, combined, 0, nonce.Length);
        Buffer.BlockCopy(ciphertext, 0, combined, nonce.Length, ciphertext.Length);
        Buffer.BlockCopy(tag, 0, combined, nonce.Length + ciphertext.Length, tag.Length);
        return Convert.ToBase64String(combined);
    }

    private byte[] DecryptPrivateKey(string encryptedBase64)
    {
        var encKey = GetEncryptionKey();
        var combined = Convert.FromBase64String(encryptedBase64);

        int nonceLen = AesGcm.NonceByteSizes.MaxSize;
        int tagLen = AesGcm.TagByteSizes.MaxSize;
        int ciphertextLen = combined.Length - nonceLen - tagLen;

        var nonce = combined.AsSpan(0, nonceLen);
        var ciphertext = combined.AsSpan(nonceLen, ciphertextLen);
        var tag = combined.AsSpan(nonceLen + ciphertextLen, tagLen);

        var plaintext = new byte[ciphertextLen];
        using var aes = new AesGcm(encKey, tagLen);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);
        return plaintext;
    }

    private byte[] GetEncryptionKey()
    {
        var keyBase64 = _config["Jwt__KeyEncryptionKey"]
            ?? throw new InvalidOperationException(
                "JWT__KeyEncryptionKey environment variable is not configured."
            );
        var key = Convert.FromBase64String(keyBase64);
        if (key.Length != 32)
        {
            throw new InvalidOperationException(
                "JWT__KeyEncryptionKey must be exactly 32 bytes (256 bits) when base64-decoded."
            );
        }

        return key;
    }

    /// <summary>Computes SHA-256 hash of a raw refresh token for safe DB storage.</summary>
    public static string HashRefreshToken(string rawToken)
    {
        var bytes = Encoding.UTF8.GetBytes(rawToken);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}
