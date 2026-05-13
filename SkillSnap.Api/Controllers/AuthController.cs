using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SkillSnap.Api.Models;

namespace SkillSnap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly IConfiguration _configuration;

	public AuthController(
		UserManager<ApplicationUser> userManager,
		SignInManager<ApplicationUser> signInManager,
		IConfiguration configuration)
	{
		_userManager = userManager;
		_signInManager = signInManager;
		_configuration = configuration;
	}

	[HttpPost("register")]
	public async Task<IActionResult> Register([FromBody] RegisterRequest request)
	{
		if (!ModelState.IsValid)
		{
			return ValidationProblem(ModelState);
		}

		var normalizedEmail = request.Email.Trim().ToLowerInvariant();
		var normalizedUserName = request.UserName.Trim();

		if (string.IsNullOrWhiteSpace(normalizedUserName))
		{
			return BadRequest(new { message = "Username is required." });
		}

		var existingUserByEmail = await _userManager.FindByEmailAsync(normalizedEmail);
		if (existingUserByEmail is not null)
		{
			return BadRequest(new { message = "Email is already in use." });
		}

		var existingUserByUserName = await _userManager.FindByNameAsync(normalizedUserName);
		if (existingUserByUserName is not null)
		{
			return BadRequest(new { message = "Username is already in use." });
		}

		var user = new ApplicationUser
		{
			UserName = normalizedUserName,
			Email = normalizedEmail,
			FullName = request.FullName?.Trim(),
			ProfileImageUrl = request.ProfileImageUrl?.Trim(),
			CreatedAt = DateTime.UtcNow,
			IsActive = true
		};

		var result = await _userManager.CreateAsync(user, request.Password);
		if (!result.Succeeded)
		{
			return BadRequest(new
			{
				message = "User registration failed.",
				errors = result.Errors.Select(e => e.Description).ToArray()
			});
		}

		return Ok(new
		{
			message = "User registered successfully.",
			user = new
			{
				user.Id,
				user.UserName,
				user.Email,
				user.FullName
			}
		});
	}

	[HttpPost("login")]
	public async Task<IActionResult> Login([FromBody] LoginRequest request)
	{
		if (!ModelState.IsValid)
		{
			return ValidationProblem(ModelState);
		}

		var normalizedEmail = request.Email.Trim().ToLowerInvariant();
		var user = await _userManager.FindByEmailAsync(normalizedEmail);
		if (user is null || !user.IsActive)
		{
			return Unauthorized(new { message = "Invalid credentials." });
		}

		var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
		if (!signInResult.Succeeded)
		{
			if (signInResult.IsLockedOut)
			{
				return Unauthorized(new { message = "User account is locked. Try again later." });
			}

			return Unauthorized(new { message = "Invalid credentials." });
		}

		var token = GenerateJwtToken(user);

		return Ok(new
		{
			token,
			expiresInMinutes = GetJwtExpireMinutes(),
			user = new
			{
				user.Id,
				user.UserName,
				user.Email,
				user.FullName
			}
		});
	}

	private string GenerateJwtToken(ApplicationUser user)
	{
		var key = _configuration["Jwt:Key"];
		var issuer = _configuration["Jwt:Issuer"];
		var audience = _configuration["Jwt:Audience"];

		if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
		{
			throw new InvalidOperationException("JWT configuration is missing. Please set Jwt:Key, Jwt:Issuer and Jwt:Audience.");
		}

		var claims = new List<Claim>
		{
			new(JwtRegisteredClaimNames.Sub, user.Id),
			new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
			new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
			new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
		};

		var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
		var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
		var expires = DateTime.UtcNow.AddMinutes(GetJwtExpireMinutes());

		var jwtToken = new JwtSecurityToken(
			issuer: issuer,
			audience: audience,
			claims: claims,
			expires: expires,
			signingCredentials: credentials);

		return new JwtSecurityTokenHandler().WriteToken(jwtToken);
	}

	private int GetJwtExpireMinutes()
	{
		var configured = _configuration["Jwt:ExpireMinutes"];
		return int.TryParse(configured, out var minutes) && minutes > 0 ? minutes : 60;
	}

	public class RegisterRequest
	{
		[Required]
		[MinLength(3)]
		[MaxLength(64)]
		public string UserName { get; set; } = string.Empty;

		[Required]
		[EmailAddress]
		public string Email { get; set; } = string.Empty;

		[Required]
		[MinLength(8)]
		public string Password { get; set; } = string.Empty;

		[MaxLength(120)]
		public string? FullName { get; set; }

		[Url]
		public string? ProfileImageUrl { get; set; }
	}

	public class LoginRequest
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; } = string.Empty;

		[Required]
		public string Password { get; set; } = string.Empty;
	}
}
