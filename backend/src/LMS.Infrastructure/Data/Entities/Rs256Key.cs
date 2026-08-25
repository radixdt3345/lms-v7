namespace LMS.Infrastructure.Data.Entities;

/// <summary>
/// RS256 key pair used for JWT signing (FR-4, FR-9). Only one key is active at a time.
/// The private key is stored encrypted at rest — never in plaintext.
/// </summary>
public sealed class Rs256Key
{
    public Guid Id { get; set; }
    public string PublicKey { get; set; } = string.Empty;

    /// <summary>Envelope-encrypted private key. Encryption key is from environment secrets only.</summary>
    public string PrivateKeyEncrypted { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
