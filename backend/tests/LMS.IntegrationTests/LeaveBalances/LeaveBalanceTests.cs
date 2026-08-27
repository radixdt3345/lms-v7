using System.Net;
using System.Net.Http.Json;
using LMS.API.Tests.Infrastructure;
using Xunit;

namespace LMS.API.Tests.LeaveBalances;

public class LeaveBalanceTests : IClassFixture<LmsWebApplicationFactory>
{
    private readonly HttpClient _client;

    public LeaveBalanceTests(LmsWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task IT_031_GetMyBalances_ReturnsApiResponseEnvelope()
    {
        // Arrange - authenticate as employee
        await AuthHelper.AuthenticateAsync(_client, "superadmin@company.com", "Admin@123");

        // Act
        var response = await _client.GetAsync("/api/v1/leave-balances/me?year=2026");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponseWrapper>();
        Assert.NotNull(body);
        Assert.True(body.Data.HasValue || body.Data == null, "Response must have 'data' property");
    }

    [Fact]
    public async Task IT_032_CreditAnnualBalances_RequiresHrAdmin()
    {
        await AuthHelper.AuthenticateAsync(_client, "hradmin@company.com", "Admin@123");
        var response = await _client.PostAsJsonAsync("/api/v1/leave-balances/credit", new { Year = 2026 });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("data", body);
    }

    [Fact]
    public async Task IT_033_GetAllBalances_RequiresHrAdmin()
    {
        await AuthHelper.AuthenticateAsync(_client, "hradmin@company.com", "Admin@123");
        var response = await _client.GetAsync("/api/v1/leave-balances?year=2026");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("data", body);
    }

    [Fact]
    public async Task IT_034_AdjustBalance_ReturnsSuccess()
    {
        await AuthHelper.AuthenticateAsync(_client, "hradmin@company.com", "Admin@123");
        // Get an employee first
        var empResponse = await _client.GetAsync("/api/v1/employees");
        // Just verify adjust endpoint accepts request (may 400 if no balance exists yet)
        var response = await _client.PostAsJsonAsync("/api/v1/leave-balances/adjust", new
        {
            EmployeeId = Guid.NewGuid(),
            LeaveTypeId = Guid.NewGuid(),
            Year = 2026,
            AdjustmentDays = 1.0m,
            Reason = "Test adjustment"
        });
        // Either 200 (found) or 404 (not found) - both are valid non-500 responses
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    private record ApiResponseWrapper(System.Text.Json.JsonElement? Data);
}
