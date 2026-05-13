using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SkillSnap.Client.Models;

namespace SkillSnap.Client.Services;

public class AuthService
{
	private const string TokenStorageKey = "skillsnap.auth.token";
	private readonly HttpClient _httpClient;
	private readonly IJSRuntime _jsRuntime;
	private readonly NavigationManager _navigationManager;
	private readonly UserSessionService _userSessionService;
	private string? _token;

	public AuthService(HttpClient httpClient, IJSRuntime jsRuntime, NavigationManager navigationManager, UserSessionService userSessionService)
	{
		_httpClient = httpClient;
		_jsRuntime = jsRuntime;
		_navigationManager = navigationManager;
		_userSessionService = userSessionService;
	}

	public bool IsAuthenticated => !string.IsNullOrWhiteSpace(_token);
	public string? UserName { get; private set; }
	public string? Email { get; private set; }
	public event Action? AuthStateChanged;

	public async Task InitializeAsync()
	{
		var storedToken = await _jsRuntime.InvokeAsync<string?>("skillSnapAuth.getToken", TokenStorageKey);
		if (string.IsNullOrWhiteSpace(storedToken))
		{
			return;
		}

		if (IsExpired(storedToken))
		{
			await LogoutAsync(false);
			return;
		}

		SetAuthenticatedState(storedToken);
		SyncSessionFromToken(storedToken);
		AuthStateChanged?.Invoke();
	}

	public async Task<ApiResult<bool>> RegisterAsync(RegisterRequest request)
	{
		var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
		if (response.IsSuccessStatusCode)
		{
			return ApiResult<bool>.Ok(true);
		}

		var error = await ReadErrorOrDefaultAsync(response, "Falha ao registrar usuário.");
		return ApiResult<bool>.Fail(error, (int)response.StatusCode);
	}

	public async Task<ApiResult<bool>> LoginAsync(LoginRequest request)
	{
		var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
		if (!response.IsSuccessStatusCode)
		{
			var error = await ReadErrorOrDefaultAsync(response, "Falha ao autenticar usuário.");
			return ApiResult<bool>.Fail(error, (int)response.StatusCode);
		}

		var payload = await response.Content.ReadFromJsonAsync<LoginResponse>();
		if (payload is null || string.IsNullOrWhiteSpace(payload.Token))
		{
			return ApiResult<bool>.Fail("Resposta de login inválida.");
		}

		await _jsRuntime.InvokeVoidAsync("skillSnapAuth.setToken", TokenStorageKey, payload.Token);
		SetAuthenticatedState(payload.Token);
		SyncSessionFromToken(payload.Token);
		AuthStateChanged?.Invoke();
		return ApiResult<bool>.Ok(true);
	}

	public async Task LogoutAsync(bool navigateToLogin = true)
	{
		await _jsRuntime.InvokeVoidAsync("skillSnapAuth.removeToken", TokenStorageKey);
		_token = null;
		UserName = null;
		Email = null;
		_httpClient.DefaultRequestHeaders.Authorization = null;
		_userSessionService.Clear();
		AuthStateChanged?.Invoke();

		if (navigateToLogin)
		{
			_navigationManager.NavigateTo("/login", true);
		}
	}

	public Task EnsureAuthorizationHeaderAsync()
	{
		if (!string.IsNullOrWhiteSpace(_token))
		{
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
		}

		return Task.CompletedTask;
	}

	private void SetAuthenticatedState(string token)
	{
		_token = token;
		_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
		ReadIdentityFromToken(token);
	}

	private void SyncSessionFromToken(string token)
	{
		var payload = ReadJwtPayload(token);
		var userId = payload is not null && payload.TryGetValue("sub", out var subValue) ? subValue.GetString() : null;
		var role = ReadRoleFromPayload(payload);
		_userSessionService.SetUser(userId, UserName, Email, role);
	}

	private void ReadIdentityFromToken(string token)
	{
		var payload = ReadJwtPayload(token);
		if (payload is null)
		{
			UserId = null;
			UserName = null;
			Email = null;
			Role = null;
			return;
		}

		UserId = payload.TryGetValue("sub", out var userId) ? userId.GetString() : null;
		UserName = payload.TryGetValue("unique_name", out var userName) ? userName.GetString() : null;
		Email = payload.TryGetValue("email", out var email) ? email.GetString() : null;
		Role = ReadRoleFromPayload(payload);
	}

	private string? UserId { get; set; }
	public string? Role { get; private set; }

	private static bool IsExpired(string token)
	{
		var payload = ReadJwtPayload(token);
		if (payload is null || !payload.TryGetValue("exp", out var expValue))
		{
			return true;
		}

		long secondsSinceEpoch;
		if (expValue.ValueKind == JsonValueKind.Number)
		{
			if (!expValue.TryGetInt64(out secondsSinceEpoch))
			{
				return true;
			}
		}
		else if (expValue.ValueKind == JsonValueKind.String && long.TryParse(expValue.GetString(), out var parsed))
		{
			secondsSinceEpoch = parsed;
		}
		else
		{
			return true;
		}

		var expiry = DateTimeOffset.FromUnixTimeSeconds(secondsSinceEpoch);
		return expiry <= DateTimeOffset.UtcNow;
	}

	private static Dictionary<string, JsonElement>? ReadJwtPayload(string token)
	{
		var segments = token.Split('.');
		if (segments.Length < 2)
		{
			return null;
		}

		try
		{
			var payload = segments[1]
				.Replace('-', '+')
				.Replace('_', '/');

			switch (payload.Length % 4)
			{
				case 2:
					payload += "==";
					break;
				case 3:
					payload += "=";
					break;
			}

			var bytes = Convert.FromBase64String(payload);
			var json = Encoding.UTF8.GetString(bytes);
			return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
		}
		catch
		{
			return null;
		}
	}

	private static string? ReadRoleFromPayload(Dictionary<string, JsonElement>? payload)
	{
		if (payload is null)
		{
			return null;
		}

		var roleKeys = new[] { "role", "roles", System.Security.Claims.ClaimTypes.Role };
		foreach (var key in roleKeys)
		{
			if (!payload.TryGetValue(key, out var value))
			{
				continue;
			}

			if (value.ValueKind == JsonValueKind.String)
			{
				return value.GetString();
			}

			if (value.ValueKind == JsonValueKind.Array)
			{
				var first = value.EnumerateArray().FirstOrDefault();
				if (first.ValueKind == JsonValueKind.String)
				{
					return first.GetString();
				}
			}
		}

		return null;
	}

	private static async Task<string> ReadErrorOrDefaultAsync(HttpResponseMessage response, string fallback)
	{
		try
		{
			var content = await response.Content.ReadAsStringAsync();
			if (!string.IsNullOrWhiteSpace(content))
			{
				return content;
			}
		}
		catch
		{
		}

		return fallback;
	}
}
