namespace SkillSnap.Api.Models;

public class Skill
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    
    // Foreign key
    public int PortfolioUserId { get; set; }
    
    // Navigation property
    public PortfolioUser? PortfolioUser { get; set; }
}