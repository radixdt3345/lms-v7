using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LMS.IntegrationTests.Reports;

public class ReportTests : IClassFixture<LmsWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ReportTests(LmsWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // IT-052: Leave report returns CSV
    [Fact]
    public async Task IT052_LeaveReport_ReturnsCSV()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestTokens.HrAdmin);
        var res = await _client.GetAsync("/api/v1/reports/leave");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Contains("text/csv", res.Content.Headers.ContentType?.MediaType);
        var body = await res.Content.ReadAsStringAsync();
        Assert.Contains("EmployeeName", body);
    }

    // IT-053: Comp-off report returns CSV
    [Fact]
    public async Task IT053_CompOffReport_ReturnsCSV()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestTokens.HrAdmin);
        var res = await _client.GetAsync("/api/v1/reports/comp-off");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadAsStringAsync();
        Assert.Contains("WorkedDate", body);
    }

    // IT-054: Leave balance report returns CSV
    [Fact]
    public async Task IT054_LeaveBalanceReport_ReturnsCSV()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestTokens.HrAdmin);
        var res = await _client.GetAsync("/api/v1/reports/leave-balances");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadAsStringAsync();
        Assert.Contains("AvailableDays", body);
    }

    // IT-055: Employee cannot access reports
    [Fact]
    public async Task IT055_Employee_CannotAccessReports()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestTokens.Employee);
        var res = await _client.GetAsync("/api/v1/reports/leave");
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }
}
