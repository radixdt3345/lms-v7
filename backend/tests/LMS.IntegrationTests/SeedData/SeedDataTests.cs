using System.Net;
using System.Net.Http.Json;
using LMS.API.Tests.Infrastructure;
using Xunit;

namespace LMS.API.Tests.SeedData;

public class SeedDataTests : IClassFixture<LmsWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SeedDataTests(LmsWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task IT_068_SystemHealth_ReturnsHealthyStatus()
    {
        var response = await _client.GetAsync("/api/v1/system/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("data", body);
        Assert.Contains("Healthy", body);
    }

    [Fact]
    public async Task IT_069_SystemInfo_ReturnsVersionInfo()
    {
        var response = await _client.GetAsync("/api/v1/system/info");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("data", body);
        Assert.Contains("1.0.0", body);
    }

    [Fact]
    public async Task IT_070_SuperAdminCanLogin()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { Email = "superadmin@company.com", Password = "Admin@123" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("data", body);
    }
}
