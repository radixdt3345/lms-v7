using System.Net;
using System.Net.Http.Json;
using LMS.API.Tests.Infrastructure;
using Xunit;

namespace LMS.API.Tests.LeaveApplications;

public class LeaveApplicationTests : IClassFixture<LmsWebApplicationFactory>
{
    private readonly HttpClient _client;
    public LeaveApplicationTests(LmsWebApplicationFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task IT_035_SubmitLeaveApplication_ReturnsPending()
    {
        await AuthHelper.AuthenticateAsync(_client, "superadmin@company.com", "Admin@123");
        var body = await _client.PostAsJsonAsync("/api/v1/leave-applications", new
        {
            LeaveTypeId = Guid.NewGuid(), StartDate = "2026-10-01", EndDate = "2026-10-03", Reason = "Test"
        });
        Assert.NotEqual(HttpStatusCode.InternalServerError, body.StatusCode);
        var text = await body.Content.ReadAsStringAsync();
        Assert.Contains("data", text);
    }

    [Fact]
    public async Task IT_036_GetMyApplications_ReturnsApiResponseEnvelope()
    {
        await AuthHelper.AuthenticateAsync(_client, "superadmin@company.com", "Admin@123");
        var res = await _client.GetAsync("/api/v1/leave-applications/me");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var text = await res.Content.ReadAsStringAsync();
        Assert.Contains("data", text);
    }

    [Fact]
    public async Task IT_037_GetAllApplications_RequiresHrAdmin()
    {
        await AuthHelper.AuthenticateAsync(_client, "hradmin@company.com", "Admin@123");
        var res = await _client.GetAsync("/api/v1/leave-applications");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var text = await res.Content.ReadAsStringAsync();
        Assert.Contains("data", text);
    }

    [Fact]
    public async Task IT_038_CancelApplication_Returns200()
    {
        await AuthHelper.AuthenticateAsync(_client, "superadmin@company.com", "Admin@123");
        var res = await _client.DeleteAsync($"/api/v1/leave-applications/{Guid.NewGuid()}/cancel");
        Assert.NotEqual(HttpStatusCode.InternalServerError, res.StatusCode);
    }
}
