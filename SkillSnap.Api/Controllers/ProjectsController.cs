using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SkillSnap.Api.Models;

namespace SkillSnap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
	private const string ProjectsCacheKey = "projects";
	private const string ProjectsFallbackCacheKey = "projects:fallback";
	private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);
	private static readonly TimeSpan FallbackExpiration = TimeSpan.FromMinutes(30);

	private readonly SkillSnapContext _context;
	private readonly IMemoryCache _cache;
	private readonly ILogger<ProjectsController> _logger;

	public ProjectsController(SkillSnapContext context, IMemoryCache cache, ILogger<ProjectsController> logger)
	{
		_context = context;
		_cache = cache;
		_logger = logger;
	}

	[HttpGet]
	public async Task<ActionResult<IEnumerable<Project>>> GetAll()
	{
		if (_cache.TryGetValue(ProjectsCacheKey, out List<Project>? cachedProjects))
		{
			_logger.LogInformation("Projects cache HIT for key {CacheKey}.", ProjectsCacheKey);
			return Ok(cachedProjects);
		}

		_logger.LogInformation("Projects cache MISS for key {CacheKey}.", ProjectsCacheKey);

		try
		{
			var projects = await _context.Projects
				.AsNoTracking()
				.Include(project => project.PortfolioUser)
				.ToListAsync();
			SetProjectCache(projects);
			_logger.LogInformation("Projects cache refreshed with {ProjectCount} item(s).", projects.Count);
			return Ok(projects);
		}
		catch
		{
			if (_cache.TryGetValue(ProjectsFallbackCacheKey, out List<Project>? fallbackProjects))
			{
				_logger.LogWarning("Projects primary cache unavailable; fallback cache HIT for key {CacheKey}.", ProjectsFallbackCacheKey);
				return Ok(fallbackProjects);
			}

			_logger.LogError("Projects cache MISS and fallback cache unavailable.");

			return StatusCode(StatusCodes.Status503ServiceUnavailable, new
			{
				message = "Não foi possível carregar os projetos no momento e não há cache de fallback disponível."
			});
		}
	}

	[HttpPost]
	[Authorize(Roles = "Admin")]
	public async Task<ActionResult<Project>> Add(Project project)
	{
		_context.Projects.Add(project);
		await _context.SaveChangesAsync();
		var projects = await _context.Projects
			.AsNoTracking()
			.Include(item => item.PortfolioUser)
			.ToListAsync();
		SetProjectCache(projects);
		_logger.LogInformation("Projects cache refreshed after write with {ProjectCount} item(s).", projects.Count);

		return CreatedAtAction(nameof(GetAll), new { id = project.Id }, project);
	}

	private void SetProjectCache(List<Project> projects)
	{
		_cache.Set(ProjectsCacheKey, projects, CacheExpiration);
		_cache.Set(ProjectsFallbackCacheKey, projects, FallbackExpiration);
	}
}
