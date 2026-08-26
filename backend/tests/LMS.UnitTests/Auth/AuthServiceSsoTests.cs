using FluentAssertions;
using LMS.Infrastructure.Auth;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace LMS.UnitTests.Auth;

public sealed class AuthServiceSsoTests : IDisposable
{
    private readonly LmsDbContext _db;
    private readonly AuthService _sut;

    public AuthServiceSsoTests()
    {
        var options = new DbContextOptionsBuilder<LmsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new LmsDbContext(options);

        var configValues = new Dictionary<string, string?>
        {
            ["AzureAd__TenantId"] = "my-tenant-id",
            ["AzureAd__ClientId"] = "my-client-id",
            ["AzureAd__ClientSecret"] = "my-secret",
            ["AzureAd__RedirectUri"] = "https://app.example.com/api/v1/auth/sso/callback",
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();
        _sut = new AuthService(_db, new Mock<IJwtService>().Object, config, new HttpClient());
    }

    public void Dispose() => _db.Dispose();

    // UT-API-027
    [Fact]
    public void GetSsoAuthorizationUrl_ReturnsWellFormedAzureAdUrl()
    {
        var url = _sut.GetSsoAuthorizationUrl(state: "csrf-token-123");

        url.Should()
            .StartWith(
                "https://login.microsoftonline.com/my-tenant-id/oauth2/v2.0/authorize"
            );
        url.Should().Contain("client_id=my-client-id");
        url.Should().Contain("response_type=code");
        url.Should().Contain("scope=");
        url.Should().Contain("state=csrf-token-123");
    }

    [Fact]
    public void GetSsoAuthorizationUrl_WithoutState_OmitsStateParam()
    {
        var url = _sut.GetSsoAuthorizationUrl();
        url.Should().NotContain("state=");
    }
}
