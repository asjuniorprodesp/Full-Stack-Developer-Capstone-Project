using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Api.Models;
using SkillSnap.Api;

namespace SkillSnap.Api.Controllers;

[ApiController]
[Route("api/profile")]
public class ProfileController : ControllerBase
{
    private readonly SkillSnapContext _context;
    public ProfileController(SkillSnapContext context)
    {
        _context = context;
    }

    [HttpGet]
    public ActionResult<PortfolioUser> GetProfile()
    {
        var user = _context.PortfolioUsers.FirstOrDefault();
        if (user == null)
            return NotFound();
        return user;
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<PortfolioUser>>> GetProfiles()
    {
        var users = await _context.PortfolioUsers
            .AsNoTracking()
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
        return profile;
    }
}