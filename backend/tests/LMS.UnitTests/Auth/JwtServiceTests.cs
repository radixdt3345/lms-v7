using FluentAssertions;
using LMS.Infrastructure.Auth;
using LMS.Infrastructure.Data;
using LMS.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Xunit;

namespace LMS.UnitTests.Auth;

public sealed class JwtServiceTests : IDisposable
{
    private readonly LmsDbContext _db;
    private readonly IConfiguration _config;
    private readonly JwtService _sut;

    public JwtServiceTests()
    {
        var options = new DbContextOptionsBuilder<LmsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new LmsDbContext(options);

        var aesKey = new byte[32];
        RandomNumberGenerator.Fill(aesKey);
        var aesKeyBase64 = Convert.ToBase64String(aesKey);

        var configValues = new Dictionary<string, string?>
        {
            ["Jwt__Issuer"] = "test-issuer",
            ["Jwt__Audience"] = "test-audience",
            ["Jwt__KeyEncryptionKey"] = aesKeyBase64,
        };
        _config = new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();
        _sut = new JwtService(_db, _config);
    }

    public void Dispose() => _db.Dispose();

    // UT-API-001
    [Fact]
    public async Task EnsureActiveKeyAsync_WhenNoKeyExists_CreatesAndPersistsKeyPair()
    {
        await _sut.EnsureActiveKeyAsync();

        var key = await _db.Rs256Keys.SingleOrDefaultAsync();
        key.Should().NotBeNull();
        key!.IsActive.Should().BeTrue();
        key.PublicKey.Should().NotBeNullOrEmpty();
        key.PrivateKeyEncrypted.Should().NotBeNullOrEmpty();
    }

    // UT-API-002
    [Fact]
    public async Task EnsureActiveKeyAsync_WhenKeyAlreadyExists_DoesNotCreateDuplicate()
    {
        await _sut.EnsureActiveKeyAsync();
        await _sut.EnsureActiveKeyAsync();

        var count = await _db.Rs256Keys.CountAsync();
        count.Should().Be(1);
    }

    // UT-API-003
    [Fact]
    public async Task GenerateAccessTokenAsync_ReturnsValidJwt()
    {
        await _sut.EnsureActiveKeyAsync();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Alice Smith",
            Email = "alice@example.com",
            Role = "EMPLOYEE",
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        var token = await _sut.GenerateAccessTokenAsync(user);

        token.Should().NotBeNullOrEmpty();
        var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(token).Should().BeTrue();

        var jwt = handler.ReadJwtToken(token);
        jwt.Issuer.Should().Be("test-issuer");
        jwt.Audiences.Should().Contain("test-audience");
        jwt.Claims.Should().Contain(c => c.Type == "role" && c.Value == "EMPLOYEE");
        jwt.Claims.Should().Contain(
            c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id.ToString()
        );
        jwt.Claims.Should().Contain(
            c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email
        );
    }

    // UT-API-004
    [Fact]
    public async Task GenerateAccessTokenAsync_TokenExpiresIn24Hours()
    {
        await _sut.EnsureActiveKeyAsync();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Bob",
            Email = "bob@example.com",
            Role = "MANAGER",
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        var before = DateTime.UtcNow;
        var token = await _sut.GenerateAccessTokenAsync(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.ValidTo.Should().BeCloseTo(before.AddHours(24), TimeSpan.FromMinutes(1));
    }

    // UT-API-005
    [Fact]
    public void GenerateRawRefreshToken_ReturnsUniqueNonEmptyValues()
    {
        var token1 = _sut.GenerateRawRefreshToken();
        var token2 = _sut.GenerateRawRefreshToken();

        token1.Should().NotBeNullOrEmpty();
        token2.Should().NotBeNullOrEmpty();
        token1.Should().NotBe(token2);
    }

    // UT-API-005b
    [Fact]
    public void HashRefreshToken_IsDeterministic()
    {
        var raw = _sut.GenerateRawRefreshToken();
        var hash1 = JwtService.HashRefreshToken(raw);
        var hash2 = JwtService.HashRefreshToken(raw);

        hash1.Should().Be(hash2);
        hash1.Should().NotBe(raw);
    }
}
