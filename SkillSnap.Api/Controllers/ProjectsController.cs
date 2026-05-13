using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Api.Models;

namespace SkillSnap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
	private readonly SkillSnapContext _context;

	public ProjectsController(SkillSnapContext context)
	{
		_context = context;
	}

	[HttpGet]
	public async Task<ActionResult<IEnumerable<Project>>> GetAll()
	{
		var projects = await _context.Projects.ToListAsync();
		return Ok(projects);
	}

	[HttpPost]
	[Authorize(Roles = "Admin")]
	public async Task<ActionResult<Project>> Add(Project project)
	{
		_context.Projects.Add(project);
		await _context.SaveChangesAsync();

		return CreatedAtAction(nameof(GetAll), new { id = project.Id }, project);
	}
}
