using System.Net.Http.Headers;
using Client.Models;

namespace Client.Services;

public class NotificationService
{
    private readonly HttpClient _http;
    private readonly AuthService _auth;

    public NotificationService(HttpClient http, AuthService auth)
    {
        _http = http;
        _auth = auth;
    }

    public async Task SubscribeAsync(Guid adId)
    {
        SetAuthHeader();
        var response = await _http.PostAsync($"api/notify/{adId}", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task UnsubscribeAsync(Guid adId)
    {
        SetAuthHeader();
        var response = await _http.DeleteAsync($"api/notify/{adId}");
        response.EnsureSuccessStatusCode();
    }

    private void SetAuthHeader()
    {
        var token = _auth.GetToken();
        if (token is not null)
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}
