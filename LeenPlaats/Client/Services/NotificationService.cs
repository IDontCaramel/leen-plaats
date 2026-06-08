using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.JSInterop;

namespace Client.Services;

public class NotificationService
{
    private readonly HttpClient _http;
    private readonly AuthService _auth;
    private readonly IJSRuntime _js;
    private string? _vapidPublicKey;

    public NotificationService(HttpClient http, AuthService auth, IJSRuntime js)
    {
        _http = http;
        _auth = auth;
        _js = js;
    }

    public async Task SubscribeAsync(Guid adId)
    {
        SetAuthHeader();
        await EnsurePushRegisteredAsync();
        var response = await _http.PostAsync($"api/notify/{adId}", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task UnsubscribeAsync(Guid adId)
    {
        SetAuthHeader();
        var response = await _http.DeleteAsync($"api/notify/{adId}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<bool> IsSubscribedAsync(Guid adId)
    {
        SetAuthHeader();
        var response = await _http.GetAsync($"api/notify/{adId}");
        return response.IsSuccessStatusCode;
    }

    public async Task UnregisterPushAsync()
    {
        await _js.InvokeVoidAsync("unsubscribeFromPush");
    }

    public async Task EnsurePushRegisteredAsync()
    {
        if (_vapidPublicKey is null)
            _vapidPublicKey = await FetchVapidPublicKeyAsync();

        var subscriptionJson = await _js.InvokeAsync<string?>("subscribeToPush", _vapidPublicKey);
        if (subscriptionJson is null) return;

        var parsed = JsonSerializer.Deserialize<PushSubJson>(subscriptionJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (parsed is null) return;

        SetAuthHeader();
        await _http.PostAsJsonAsync("api/push/subscribe", new
        {
            parsed.Endpoint,
            P256DH = parsed.Keys.P256dh,
            Auth = parsed.Keys.Auth
        });
    }

    private async Task<string> FetchVapidPublicKeyAsync()
    {
        var response = await _http.GetAsync("api/push/vapid-public-key");
        response.EnsureSuccessStatusCode();
        var key = await response.Content.ReadFromJsonAsync<string>();
        return key!;
    }

    private void SetAuthHeader()
    {
        var token = _auth.GetToken();
        if (token is not null)
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private record PushSubJson(string Endpoint, PushSubKeys Keys);
    private record PushSubKeys(string P256dh, string Auth);
}
