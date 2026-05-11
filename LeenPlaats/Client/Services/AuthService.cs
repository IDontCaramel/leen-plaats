using Microsoft.JSInterop;

namespace Client.Services;

public class AuthService
{
    private readonly IJSRuntime _js;
    private string? _token;
    private string? _userId;
    private string? _displayName;

    public AuthService(IJSRuntime js) => _js = js;

    public bool IsAuthenticated => _token is not null;
    public string? DisplayName => _displayName;
    public string? UserId => _userId;

    public event Action? OnChange;

    public async Task InitializeAsync()
    {
        _token = await _js.InvokeAsync<string?>("localStorage.getItem", "jwt");
        _userId = await _js.InvokeAsync<string?>("localStorage.getItem", "userId");
        _displayName = await _js.InvokeAsync<string?>("localStorage.getItem", "displayName");
    }

    public async Task LoginAsync(string token, string userId, string displayName)
    {
        _token = token;
        _userId = userId;
        _displayName = displayName;
        await _js.InvokeVoidAsync("localStorage.setItem", "jwt", token);
        await _js.InvokeVoidAsync("localStorage.setItem", "userId", userId);
        await _js.InvokeVoidAsync("localStorage.setItem", "displayName", displayName);
        OnChange?.Invoke();
    }

    public async Task LogoutAsync()
    {
        _token = null;
        _userId = null;
        _displayName = null;
        await _js.InvokeVoidAsync("localStorage.removeItem", "jwt");
        await _js.InvokeVoidAsync("localStorage.removeItem", "userId");
        await _js.InvokeVoidAsync("localStorage.removeItem", "displayName");
        OnChange?.Invoke();
    }

    public string? GetToken() => _token;
}
