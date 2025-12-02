
using LoginPageAPI.Data;
using LoginPageAPI.DTOs;
using LoginPageAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LoginPageAPI.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthController(ApplicationDbContext context, IConfiguration configuration, IPasswordHasher<User> passwordHasher) 
    {
        _context = context;
        _configuration = configuration;
        _passwordHasher = passwordHasher;
    }


    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req) 
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);


        if (await _context.Users.AnyAsync(u => u.Username == req.Username))
            return Conflict(new { message = "Username already exist" });

        var user = new User { Username = req.Username };
        user.PasswordHash = _passwordHasher.HashPassword(user, req.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Created("", new { message = "User registered successfully." });
    }


    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest login) 
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);


        var user = await _context.Users
            .SingleOrDefaultAsync(u => u.Username == login.Username);

        if (user is null)
            return Unauthorized(new { message = "Invalid username" });

        var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, login.Password);
        if (verify == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Incorrect password" });



        var jwtSection = _configuration.GetSection("Jwt");
        var key = jwtSection["Key"];
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];
        var expiresMinutes = int.TryParse(jwtSection["ExpiresMinutes"], out var mins) ? mins : 60;


        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("uid", user.Id.ToString())
        };


        var creds = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
                signingCredentials: creds);

        var response = new AuthResponse 
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAtUtc = token.ValidTo
        };


        return Ok(response);
    }
}