using System.Net.Http.Json;
using SkillSnap.Client.Models;

namespace SkillSnap.Client.Services;

public class SkillService
{
	private readonly HttpClient _httpClient;
	private readonly ILogger<SkillService> _logger;
	private const int MaxRetries = 2;

	public SkillService(HttpClient httpClient, ILogger<SkillService> logger)
	{
		_httpClient = httpClient;
		_logger = logger;
	}

	public async Task<List<Skill>> GetSkillsAsync()
	{
		var result = await GetSkillsResultAsync();
		return result.Data ?? new List<Skill>();
	}

	public Task<ApiResult<List<Skill>>> GetSkillsResultAsync()
	{
		return ExecuteWithRetryAsync(async () =>
		{
			var response = await _httpClient.GetAsync("api/skills");
			return await ParseResponseAsync<List<Skill>>(response, "carregar skills");
		});
	}

	public async Task<Skill?> AddSkillAsync(Skill newSkill)
	{
		var result = await AddSkillResultAsync(newSkill);
		return result.Data;
	}

	public Task<ApiResult<Skill>> AddSkillResultAsync(Skill newSkill)
	{
		return ExecuteWithRetryAsync(async () =>
		{
			var response = await _httpClient.PostAsJsonAsync("api/skills", newSkill);
			return await ParseResponseAsync<Skill>(response, "adicionar skill");
		});
	}

	private async Task<ApiResult<T>> ExecuteWithRetryAsync<T>(Func<Task<ApiResult<T>>> operation)
	{
		for (var attempt = 0; attempt <= MaxRetries; attempt++)
		{
			try
			{
				var result = await operation();

				if (result.Success || !IsTransientStatusCode(result.StatusCode) || attempt == MaxRetries)
				{
					return result;
				}

				await Task.Delay(GetRetryDelay(attempt));
			}
			catch (HttpRequestException ex)
			{
				_logger.LogWarning(ex, "Falha de conectividade ao acessar API de skills. Tentativa {Attempt}", attempt + 1);

				if (attempt == MaxRetries)
				{
					return ApiResult<T>.Fail("Não foi possível conectar com a API. Verifique se o backend está em execução.");
				}

				await Task.Delay(GetRetryDelay(attempt));
			}
			catch (TaskCanceledException ex)
			{
				_logger.LogWarning(ex, "Timeout ao acessar API de skills. Tentativa {Attempt}", attempt + 1);

				if (attempt == MaxRetries)
				{
					return ApiResult<T>.Fail("A requisição excedeu o tempo limite. Tente novamente.");
				}

				await Task.Delay(GetRetryDelay(attempt));
			}
		}

		return ApiResult<T>.Fail("Erro inesperado ao processar requisição da API.");
	}

	private static bool IsTransientStatusCode(int? statusCode)
	{
		return statusCode is 408 or 429 or >= 500;
	}

	private static TimeSpan GetRetryDelay(int attempt)
	{
		return TimeSpan.FromMilliseconds(300 * (attempt + 1) * (attempt + 1));
	}

	private static async Task<ApiResult<T>> ParseResponseAsync<T>(HttpResponseMessage response, string operation)
	{
		if (response.IsSuccessStatusCode)
		{
			var data = await response.Content.ReadFromJsonAsync<T>();
			if (data is null)
			{
				return ApiResult<T>.Fail($"Resposta vazia ao {operation}.", (int)response.StatusCode);
			}

			return ApiResult<T>.Ok(data);
		}

		var statusCode = (int)response.StatusCode;
		return ApiResult<T>.Fail(MapHttpErrorMessage(statusCode, operation), statusCode);
	}

	private static string MapHttpErrorMessage(int statusCode, string operation)
	{
		return statusCode switch
		{
			400 => $"Dados inválidos ao tentar {operation}.",
			401 => "Você não está autenticado para esta operação.",
			403 => "Você não tem permissão para esta operação.",
			404 => "Recurso não encontrado na API.",
			409 => "Conflito de dados detectado.",
			>= 500 => "Erro interno no servidor. Tente novamente em instantes.",
			_ => $"Falha ao {operation}. Código HTTP: {statusCode}."
		};
	}
}
