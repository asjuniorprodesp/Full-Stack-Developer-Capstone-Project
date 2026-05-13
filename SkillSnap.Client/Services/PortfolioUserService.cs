using SkillSnap.Client.Models;
using System.Net.Http.Json;

namespace SkillSnap.Client.Services;

public class PortfolioUserService
{
    private readonly HttpClient _httpClient;
    private readonly AuthService _authService;
    private readonly ILogger<PortfolioUserService> _logger;
    public PortfolioUserService(HttpClient httpClient, AuthService authService, ILogger<PortfolioUserService> logger)
    {
        _httpClient = httpClient;
        _authService = authService;
        _logger = logger;
    }

    public async Task<PortfolioUser?> GetProfileAsync()
    {
        try
        {
            await _authService.EnsureAuthorizationHeaderAsync();
            var result = await _httpClient.GetFromJsonAsync<PortfolioUser>("api/profile");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch profile");
            return null;
        }
    }

    public async Task<List<PortfolioUser>> GetProfilesAsync()
    {
        try
        {
            await _authService.EnsureAuthorizationHeaderAsync();
            var result = await _httpClient.GetFromJsonAsync<List<PortfolioUser>>("api/profile/all");
            return result ?? new List<PortfolioUser>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch profiles");
            return new List<PortfolioUser>();
        }
    }
}