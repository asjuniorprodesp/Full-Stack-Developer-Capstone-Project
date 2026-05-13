using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SkillSnap.Api.Models;
using SkillSnap.Api;

namespace SkillSnap.Api.Controllers;

[ApiController]
[Route("api/profile")]
public class ProfileController : ControllerBase
{
    private readonly SkillSnapContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(SkillSnapContext context, IMemoryCache cache, ILogger<ProfileController> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<PortfolioUser> GetProfile()
    {
        var user = _context.PortfolioUsers
            .AsNoTracking()
            .Include(profile => profile.Projects)
            .Include(profile => profile.Skills)
            .FirstOrDefault();
        if (user == null)
            return NotFound();
        return user;
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<PortfolioUser>>> GetProfiles()
    {
        var users = await _context.PortfolioUsers
            .AsNoTracking()
            .Include(profile => profile.Projects)
            .Include(profile => profile.Skills)
            .OrderBy(user => user.Id)
            .ToListAsync();

        return Ok(users);
    }

    [HttpPost]
    public ActionResult<PortfolioUser> CreateOrUpdateProfile(PortfolioUser profile)
    {
        var user = _context.PortfolioUsers.FirstOrDefault();
        if (user == null)
        {
            _context.PortfolioUsers.Add(profile);
        }
        else
        {
            user.Name = profile.Name;
            user.Bio = profile.Bio;
            user.ProfileImageUrl = profile.ProfileImageUrl;
        }
        _context.SaveChanges();

        _cache.Remove("projects");
        _cache.Remove("projects:fallback");
        _cache.Remove("skills");
        _cache.Remove("skills:fallback");
        _logger.LogInformation("Profile updated; project and skill caches invalidated.");

        return profile;
    }
}