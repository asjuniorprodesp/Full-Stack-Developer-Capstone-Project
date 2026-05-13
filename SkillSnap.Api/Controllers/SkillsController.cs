using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Api.Models;

namespace SkillSnap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkillsController : ControllerBase
{
	private readonly SkillSnapContext _context;

	public SkillsController(SkillSnapContext context)
	{
		_context = context;
	}

	[HttpGet]
	public async Task<ActionResult<IEnumerable<Skill>>> GetAll()
	{
		var skills = await _context.Skills.ToListAsync();
		return Ok(skills);
	}

	[HttpPost]
	public async Task<ActionResult<Skill>> Add(Skill skill)
	{
		_context.Skills.Add(skill);
		await _context.SaveChangesAsync();

		return CreatedAtAction(nameof(GetAll), new { id = skill.Id }, skill);
	}
}
