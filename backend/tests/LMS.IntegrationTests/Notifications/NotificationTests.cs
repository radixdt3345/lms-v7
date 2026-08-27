using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LMS.IntegrationTests.Notifications;

public class NotificationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public NotificationTests(WebApplicationFactory<Program> factory) => _client = factory.CreateClient();

    // IT-046: Authenticated user gets notifications
    [Fact]
    public async Task IT046_GetNotifications_ReturnsApiResponse()
    {
        var token = await TestHelpers.GetEmployeeTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync("/api/v1/notifications");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponseWrapper<List<object>>>();
        Assert.NotNull(body);
        Assert.NotNull(body!.Data);
    }

    // IT-047: Unread count returns ApiResponse<int>
    [Fact]
    public async Task IT047_GetUnreadCount_ReturnsApiResponse()
    {
        var token = await TestHelpers.GetEmployeeTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync("/api/v1/notifications/unread-count");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponseWrapper<int>>();
        Assert.NotNull(body);
    }

    // IT-048: Unauthenticated request returns 401
    [Fact]
    public async Task IT048_UnauthenticatedRequest_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/notifications");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
internal record ApiResponseWrapper<T>(T? Data);
