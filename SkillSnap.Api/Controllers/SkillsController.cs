using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SkillSnap.Api.Models;

namespace SkillSnap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkillsController : ControllerBase
{
	private const string SkillsCacheKey = "skills";
	private const string SkillsFallbackCacheKey = "skills:fallback";
	private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);
	private static readonly TimeSpan FallbackExpiration = TimeSpan.FromMinutes(30);

	private readonly SkillSnapContext _context;
	private readonly IMemoryCache _cache;
	private readonly ILogger<SkillsController> _logger;

	public SkillsController(SkillSnapContext context, IMemoryCache cache, ILogger<SkillsController> logger)
	{
		_context = context;
		_cache = cache;
		_logger = logger;
	}

	[HttpGet]
	public async Task<ActionResult<IEnumerable<Skill>>> GetAll()
	{
		if (_cache.TryGetValue(SkillsCacheKey, out List<Skill>? cachedSkills))
		{
			_logger.LogInformation("Skills cache HIT for key {CacheKey}.", SkillsCacheKey);
			return Ok(cachedSkills);
		}

		_logger.LogInformation("Skills cache MISS for key {CacheKey}.", SkillsCacheKey);

		try
		{
			var skills = await _context.Skills
				.AsNoTracking()
				.Include(skill => skill.PortfolioUser)
				.ToListAsync();
			SetSkillCache(skills);
			_logger.LogInformation("Skills cache refreshed with {SkillCount} item(s).", skills.Count);
			return Ok(skills);
		}
		catch
		{
			if (_cache.TryGetValue(SkillsFallbackCacheKey, out List<Skill>? fallbackSkills))
			{
				_logger.LogWarning("Skills primary cache unavailable; fallback cache HIT for key {CacheKey}.", SkillsFallbackCacheKey);
				return Ok(fallbackSkills);
			}

			_logger.LogError("Skills cache MISS and fallback cache unavailable.");

			return StatusCode(StatusCodes.Status503ServiceUnavailable, new
			{
				message = "Não foi possível carregar as skills no momento e não há cache de fallback disponível."
			});
		}
	}

	[HttpPost]
	[Authorize(Roles = "Admin")]
	public async Task<ActionResult<Skill>> Add(Skill skill)
	{
		_context.Skills.Add(skill);
		await _context.SaveChangesAsync();
		var skills = await _context.Skills
			.AsNoTracking()
			.Include(item => item.PortfolioUser)
			.ToListAsync();
		SetSkillCache(skills);
		_logger.LogInformation("Skills cache refreshed after write with {SkillCount} item(s).", skills.Count);

		return CreatedAtAction(nameof(GetAll), new { id = skill.Id }, skill);
	}

	private void SetSkillCache(List<Skill> skills)
	{
		_cache.Set(SkillsCacheKey, skills, CacheExpiration);
		_cache.Set(SkillsFallbackCacheKey, skills, FallbackExpiration);
	}
}
