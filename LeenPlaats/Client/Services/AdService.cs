using System.Net.Http.Headers;
using System.Net.Http.Json;
using Client.Models;

namespace Client.Services;

public class AdService
{
    private readonly HttpClient _http;
    private readonly AuthService _auth;

    public AdService(HttpClient http, AuthService auth)
    {
        _http = http;
        _auth = auth;
    }

    public async Task<List<AdDto>> GetAdsAsync(string? search = null, string? category = null, bool onlyAvailable = false)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search)}");
        if (!string.IsNullOrWhiteSpace(category)) query.Add($"category={Uri.EscapeDataString(category)}");
        if (onlyAvailable) query.Add("onlyAvailable=true");

        var url = "api/ads" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
        return await _http.GetFromJsonAsync<List<AdDto>>(url) ?? [];
    }

    public async Task<AdDto?> GetAdAsync(Guid id) =>
        await _http.GetFromJsonAsync<AdDto>($"api/ads/{id}");

    public async Task<List<LendRequestDto>> GetRequestsForAdAsync(Guid adId)
    {
        SetAuthHeader();
        return await _http.GetFromJsonAsync<List<LendRequestDto>>($"api/ads/{adId}/requests") ?? [];
    }

    public async Task<AdDto?> CreateAdAsync(CreateAdDto dto)
    {
        SetAuthHeader();
        var response = await _http.PostAsJsonAsync("api/ads", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AdDto>();
    }

    public async Task UploadPhotoAsync(Guid adId, Stream stream, string fileName)
    {
        SetAuthHeader();
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(streamContent, "file", fileName);
        var response = await _http.PostAsync($"api/ads/{adId}/photos", content);
        response.EnsureSuccessStatusCode();
    }

    public async Task SetAvailabilityAsync(Guid adId, bool isAvailable)
    {
        SetAuthHeader();
        var response = await _http.PatchAsJsonAsync($"api/ads/{adId}/availability", new { IsAvailable = isAvailable });
        response.EnsureSuccessStatusCode();
    }

    public async Task<LendRequestDto?> CreateLendRequestAsync(CreateLendRequestDto dto)
    {
        SetAuthHeader();
        var response = await _http.PostAsJsonAsync("api/requests", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<LendRequestDto>();
    }

    public async Task AcceptRequestAsync(Guid requestId)
    {
        SetAuthHeader();
        var response = await _http.PutAsync($"api/requests/{requestId}/accept", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeclineRequestAsync(Guid requestId)
    {
        SetAuthHeader();
        var response = await _http.PutAsync($"api/requests/{requestId}/decline", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<AdDto>> GetMyAdsAsync()
    {
        SetAuthHeader();
        return await _http.GetFromJsonAsync<List<AdDto>>("api/ads/mine") ?? [];
    }

    public async Task DeleteAdAsync(Guid adId)
    {
        SetAuthHeader();
        var response = await _http.DeleteAsync($"api/ads/{adId}");
        response.EnsureSuccessStatusCode();
    }

    private void SetAuthHeader()
    {
        var token = _auth.GetToken();
        if (token is not null)
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}
