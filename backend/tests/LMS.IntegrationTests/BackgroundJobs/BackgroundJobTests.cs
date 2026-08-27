using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace LMS.IntegrationTests.BackgroundJobs;

public class BackgroundJobTests : IClassFixture<LmsWebApplicationFactory>
{
    private readonly HttpClient _client;

    public BackgroundJobTests(LmsWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // IT-056: Expire comp-off credits
    [Fact]
    public async Task IT056_ExpireCompOff_ReturnsSuccess()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestTokens.HrAdmin);
        var res = await _client.PostAsync("/api/v1/jobs/expire-comp-off", null);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadAsStringAsync();
        Assert.Contains("data", body);
    }

    // IT-057: Reset leave balances
    [Fact]
    public async Task IT057_ResetLeaveBalances_ReturnsSuccess()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestTokens.HrAdmin);
        var res = await _client.PostAsync("/api/v1/jobs/reset-leave-balances?year=2099", null);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadAsStringAsync();
        Assert.Contains("data", body);
    }

    // IT-058: Send reminders
    [Fact]
    public async Task IT058_SendReminders_ReturnsSuccess()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestTokens.HrAdmin);
        var res = await _client.PostAsync("/api/v1/jobs/send-reminders", null);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    // IT-059: Job logs are retrievable
    [Fact]
    public async Task IT059_GetJobLogs_ReturnsLogs()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestTokens.HrAdmin);
        // Run a job first
        await _client.PostAsync("/api/v1/jobs/send-reminders", null);
        var res = await _client.GetAsync("/api/v1/jobs/logs");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadAsStringAsync();
        Assert.Contains("data", body);
    }

    // IT-060: Employee cannot trigger jobs
    [Fact]
    public async Task IT060_Employee_CannotTriggerJobs()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestTokens.Employee);
        var res = await _client.PostAsync("/api/v1/jobs/expire-comp-off", null);
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }
}
