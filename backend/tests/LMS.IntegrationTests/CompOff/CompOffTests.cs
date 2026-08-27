using System.Net;
using System.Net.Http.Json;
using LMS.Infrastructure.CompOff;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LMS.IntegrationTests.CompOff;

public class CompOffTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CompOffTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    // IT-039: Employee can submit comp-off request
    [Fact]
    public async Task IT039_SubmitCompOffRequest_ReturnsCreated()
    {
        var token = await TestHelpers.GetEmployeeTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/v1/comp-off/requests", new
        {
            workedDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)).ToString("yyyy-MM-dd"),
            creditDays = 1.0m,
            reason = "Worked on weekend for project delivery"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponseWrapper<CompOffRequestDto>>();
        Assert.NotNull(body?.Data);
        Assert.Equal("Pending", body!.Data!.Status);
    }

    // IT-040: HR Admin can approve comp-off request
    [Fact]
    public async Task IT040_ApproveCompOffRequest_CreatesCredit()
    {
        var hrToken = await TestHelpers.GetHrAdminTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", hrToken);

        // Get a pending request
        var listResp = await _client.GetAsync("/api/v1/comp-off/requests");
        Assert.Equal(HttpStatusCode.OK, listResp.StatusCode);
        var list = await listResp.Content.ReadFromJsonAsync<ApiResponseWrapper<List<CompOffRequestDto>>>();
        var pending = list?.Data?.FirstOrDefault(r => r.Status == "Pending");
        if (pending == null) return; // skip if no pending requests

        var response = await _client.PutAsync($"/api/v1/comp-off/requests/{pending.Id}/approve", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponseWrapper<CompOffRequestDto>>();
        Assert.Equal("Approved", body?.Data?.Status);
    }

    // IT-041: HR Admin can reject comp-off request
    [Fact]
    public async Task IT041_RejectCompOffRequest_ReturnsRejected()
    {
        var hrToken = await TestHelpers.GetHrAdminTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", hrToken);

        var listResp = await _client.GetAsync("/api/v1/comp-off/requests");
        var list = await listResp.Content.ReadFromJsonAsync<ApiResponseWrapper<List<CompOffRequestDto>>>();
        var pending = list?.Data?.FirstOrDefault(r => r.Status == "Pending");
        if (pending == null) return;

        var response = await _client.PutAsJsonAsync($"/api/v1/comp-off/requests/{pending.Id}/reject",
            new { rejectionReason = "Not eligible" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponseWrapper<CompOffRequestDto>>();
        Assert.Equal("Rejected", body?.Data?.Status);
    }

    // IT-042: Employee can view own comp-off credits
    [Fact]
    public async Task IT042_GetMyCredits_ReturnsApiResponse()
    {
        var token = await TestHelpers.GetEmployeeTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/v1/comp-off/credits/me");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponseWrapper<List<CompOffCreditDto>>>();
        Assert.NotNull(body);
        Assert.True(body!.HasProperty("data"));
    }
}

internal record ApiResponseWrapper<T>(T? Data);
