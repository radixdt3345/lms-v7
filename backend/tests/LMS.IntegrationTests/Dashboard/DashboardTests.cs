using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LMS.IntegrationTests.Dashboard;
public class DashboardTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public DashboardTests(WebApplicationFactory<Program> factory) => _client = factory.CreateClient();

    [Fact] public async Task IT049_EmployeeDashboard_ReturnsApiResponse()
    {
        var token = await TestHelpers.GetEmployeeTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var r = await _client.GetAsync("/api/v1/dashboard/employee");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var body = await r.Content.ReadFromJsonAsync<ApiResponseWrapper<object>>();
        Assert.NotNull(body?.Data);
    }
    [Fact] public async Task IT050_HrDashboard_ReturnsApiResponse()
    {
        var token = await TestHelpers.GetHrAdminTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var r = await _client.GetAsync("/api/v1/dashboard/hr");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var body = await r.Content.ReadFromJsonAsync<ApiResponseWrapper<object>>();
        Assert.NotNull(body?.Data);
    }
    [Fact] public async Task IT051_EmployeeCannotAccessHrDashboard()
    {
        var token = await TestHelpers.GetEmployeeTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var r = await _client.GetAsync("/api/v1/dashboard/hr");
        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
    }
}
internal record ApiResponseWrapper<T>(T? Data);
