using System.Net;
using System.Net.Http.Json;
using LMS.Infrastructure.Approvals;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LMS.IntegrationTests.Approvals;

public class ApprovalWorkflowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public ApprovalWorkflowTests(WebApplicationFactory<Program> factory)
        => _client = factory.CreateClient();

    // IT-043: HR Admin can get pending approvals
    [Fact]
    public async Task IT043_GetPendingApprovals_ReturnsApiResponse()
    {
        var token = await TestHelpers.GetHrAdminTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/v1/approvals/pending");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponseWrapper<List<PendingApprovalDto>>>();
        Assert.NotNull(body);
        Assert.NotNull(body!.Data);
    }

    // IT-044: Employee cannot access approval queue
    [Fact]
    public async Task IT044_EmployeeCannotAccessApprovalQueue_Returns403()
    {
        var token = await TestHelpers.GetEmployeeTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/v1/approvals/pending");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // IT-045: Approval history returns ApiResponse
    [Fact]
    public async Task IT045_GetApprovalHistory_ReturnsApiResponse()
    {
        var token = await TestHelpers.GetHrAdminTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/v1/approvals/history/LeaveApplication/00000000-0000-0000-0000-000000000000");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponseWrapper<List<object>>>();
        Assert.NotNull(body);
    }
}
internal record ApiResponseWrapper<T>(T? Data);
