using Microsoft.AspNetCore.Mvc;
using SkillSnap.Api.Models;

namespace SkillSnap.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeedController : ControllerBase
    {
        private readonly SkillSnapContext _context;
        public SeedController(SkillSnapContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult Seed()
        {
            if (_context.PortfolioUsers.Any())
            {
                return BadRequest("Sample data already exists.");
            }

            var users = new List<PortfolioUser>
            {
                new PortfolioUser
                {
                    Name = "Jordan Developer",
                    Bio = "Full-stack developer passionate about learning new tech.",
                    ProfileImageUrl = "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&w=300&q=80",
                    Projects = new List<Project>
                    {
                        new Project
                        {
                            Title = "Task Tracker",
                            Description = "Manage tasks effectively with priorities and deadlines.",
                            ImageUrl = "https://images.unsplash.com/photo-1454165804606-c3d57bc86b40?auto=format&fit=crop&w=800&q=80"
                        },
                        new Project
                        {
                            Title = "Weather App",
                            Description = "Forecast weather using APIs with real-time widgets.",
                            ImageUrl = "https://images.unsplash.com/photo-1489515217757-5fd1be406fef?auto=format&fit=crop&w=800&q=80"
                        },
                        new Project
                        {
                            Title = "Finance Dashboard",
                            Description = "Track expenses and monthly goals with charts.",
                            ImageUrl = "https://images.unsplash.com/photo-1554224155-6726b3ff858f?auto=format&fit=crop&w=800&q=80"
                        }
                    },
                    Skills = new List<Skill>
                    {
                        new Skill { Name = "C#", Level = "Advanced" },
                        new Skill { Name = "Blazor", Level = "Intermediate" },
                        new Skill { Name = "SQL", Level = "Intermediate" },
                        new Skill { Name = "Docker", Level = "Basic" }
                    }
                },
                new PortfolioUser
                {
                    Name = "Patricia Oliveira",
                    Bio = "Backend engineer focused on scalable APIs and cloud architecture.",
                    ProfileImageUrl = "https://images.unsplash.com/photo-1544005313-94ddf0286df2?auto=format&fit=crop&w=300&q=80",
                    Projects = new List<Project>
                    {
                        new Project
                        {
                            Title = "Order API",
                            Description = "REST API for order lifecycle and billing integration.",
                            ImageUrl = "https://images.unsplash.com/photo-1460925895917-afdab827c52f?auto=format&fit=crop&w=800&q=80"
                        },
                        new Project
                        {
                            Title = "Inventory Sync",
                            Description = "Synchronize stock across warehouse systems.",
                            ImageUrl = "https://images.unsplash.com/photo-1586528116311-ad8dd3c8310d?auto=format&fit=crop&w=800&q=80"
                        }
                    },
                    Skills = new List<Skill>
                    {
                        new Skill { Name = ".NET", Level = "Advanced" },
                        new Skill { Name = "Entity Framework", Level = "Advanced" },
                        new Skill { Name = "Azure", Level = "Intermediate" }
                    }
                }
            };

            _context.PortfolioUsers.AddRange(users);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Sample data inserted.",
                users = users.Count,
                projects = users.Sum(u => u.Projects.Count),
                skills = users.Sum(u => u.Skills.Count)
            });
        }

        [HttpDelete]
        public IActionResult Clear()
        {
            var skillsCount = _context.Skills.Count();
            var projectsCount = _context.Projects.Count();
            var usersCount = _context.PortfolioUsers.Count();

            _context.Skills.RemoveRange(_context.Skills);
            _context.Projects.RemoveRange(_context.Projects);
            _context.PortfolioUsers.RemoveRange(_context.PortfolioUsers);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Database cleared. You can run seed again.",
                removedUsers = usersCount,
                removedProjects = projectsCount,
                removedSkills = skillsCount
            });
        }
    }
}