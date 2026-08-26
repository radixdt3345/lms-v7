using System.Net;
using System.Security.Cryptography;
using LMS.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace LMS.IntegrationTests.Infrastructure;

/// <summary>
/// WebApplicationFactory that uses a Testcontainers PostgreSQL instance and
/// overrides Azure AD external HTTP calls with a controllable mock.
/// Start/stop is handled by IAsyncLifetime — one container per collection.
/// </summary>
public sealed class LmsWebApplicationFactory
    : WebApplicationFactory<Program>,
        IAsyncLifetime
{
    // 32-byte AES-256 key used exclusively for integration tests
    public static readonly string TestKeyEncryptionKey = Convert.ToBase64String(
        RandomNumberGenerator.GetBytes(32)
    );

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .WithDatabase("lms_integration_test")
        .WithUsername("lms_test")
        .WithPassword("lms_test_pw")
        .Build();

    /// <summary>Swap out the handler's response for each SSO-related test.</summary>
    public MockHttpMessageHandler AzureAdHttpHandler { get; } = new();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.StopAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Database — point at the Testcontainers instance
        builder.UseSetting(
            "ConnectionStrings:DefaultConnection",
            _postgres.GetConnectionString()
        );

        // AzureAd stubs
        builder.UseSetting("AzureAd__TenantId", "test-tenant-id");
        builder.UseSetting("AzureAd__ClientId", "test-client-id");
        builder.UseSetting("AzureAd__ClientSecret", "test-client-secret");
        builder.UseSetting(
            "AzureAd__RedirectUri",
            "https://localhost/api/v1/auth/sso/callback"
        );

        // JWT config
        builder.UseSetting("Jwt__Issuer", "lms-api");
        builder.UseSetting("Jwt__Audience", "lms-client");
        builder.UseSetting("Jwt__KeyEncryptionKey", TestKeyEncryptionKey);

        builder.ConfigureServices(services =>
        {
            // Wire the mock HTTP handler for the AuthService typed client
            // (replaces the default handler that would call real Azure AD endpoints)
            services
                .AddHttpClient<
                    LMS.Infrastructure.Auth.IAuthService,
                    LMS.Infrastructure.Auth.AuthService
                >()
                .ConfigurePrimaryHttpMessageHandler(() => AzureAdHttpHandler);

            // Run EF Core migrations so the schema matches the code
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<LmsDbContext>();
            db.Database.Migrate();
        });
    }
}

/// <summary>
/// Controllable HttpMessageHandler for injecting Azure AD responses in tests.
/// Call SetJsonResponse before the test action that triggers an outbound Azure AD call.
/// </summary>
public sealed class MockHttpMessageHandler : HttpMessageHandler
{
    private Func<HttpRequestMessage, HttpResponseMessage>? _handler;

    public void SetResponse(Func<HttpRequestMessage, HttpResponseMessage> handler)
    {
        _handler = handler;
    }

    public void SetJsonResponse(string json, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _handler = _ =>
            new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(
                    json,
                    System.Text.Encoding.UTF8,
                    "application/json"
                ),
            };
    }

    public void Reset() => _handler = null;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    ) =>
        Task.FromResult(
            _handler?.Invoke(request)
                ?? new HttpResponseMessage(HttpStatusCode.InternalServerError)
        );
}
