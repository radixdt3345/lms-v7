using FluentAssertions;
using LMS.Infrastructure.Data.Entities;
using Xunit;

namespace LMS.UnitTests.Data.Entities;

/// <summary>
/// Unit tests for the Rs256Key entity class.
/// Verifies default values and nullability reflecting DB schema constraints.
/// </summary>
public sealed class Rs256KeyEntityConfigurationTests
{
    [Fact]
    public void Rs256Key_Id_IsGuid()
    {
        var key = new Rs256Key();
        key.Id.Should().BeOfType<Guid>();
    }

    [Fact]
    public void Rs256Key_IsActive_DefaultsToTrue()
    {
        // Matches DB column default: is_active BOOLEAN DEFAULT TRUE
        // Only one key should be active at a time (enforced at application layer)
        var key = new Rs256Key();
        key.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Rs256Key_PublicKey_DefaultsToEmptyString()
    {
        // public_key TEXT NOT NULL in the DB
        var key = new Rs256Key();
        key.PublicKey.Should().Be(string.Empty);
    }

    [Fact]
    public void Rs256Key_PrivateKeyEncrypted_DefaultsToEmptyString()
    {
        // private_key_encrypted TEXT NOT NULL in the DB — stored envelope-encrypted, never plaintext
        var key = new Rs256Key();
        key.PrivateKeyEncrypted.Should().Be(string.Empty);
    }

    [Fact]
    public void Rs256Key_CanDeactivateKey()
    {
        // Key rotation: deactivate old key before activating new one
        var key = new Rs256Key { IsActive = false };
        key.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Rs256Key_CanSetPublicAndPrivateKey()
    {
        const string publicKey = "-----BEGIN PUBLIC KEY-----\nMIIBIjAN...\n-----END PUBLIC KEY-----";
        const string encryptedPrivateKey = "encrypted-payload-base64";

        var key = new Rs256Key { PublicKey = publicKey, PrivateKeyEncrypted = encryptedPrivateKey };

        key.PublicKey.Should().Be(publicKey);
        key.PrivateKeyEncrypted.Should().Be(encryptedPrivateKey);
    }
}
