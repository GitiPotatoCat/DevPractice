using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MotorsShop.Data;
using MotorsShop.Dtos;
using MotorsShop.Exceptions;
using MotorsShop.Models;
using MotorsShop.Services;
using MotorsShop.Services.Email;
using System.Security.Claims;

namespace MotorsShop.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly MotorsShopDbContext _db;
    private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;


    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        MotorsShopDbContext db,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _db = db;
        _configuration = configuration;
    }


    // AuthController.Register — replace the body with this
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        if (await _userManager.FindByEmailAsync(dto.Email) is not null)
            throw new ConflictException("A user with this email already exists.");

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded) {
            var errors = result.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
            throw new MotorsShop.Exceptions.ValidationException(errors);
        }

        await _userManager.AddToRoleAsync(user, "Customer");

        // Create the linked Customer record
        var customer = new Customer
        {
            FullName = dto.FullName,
            Email = dto.Email,
            ApplicationUserId = user.Id
        };
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();

        var token = await _tokenService.CreateTokenAsync(user);
        var roles = await _userManager.GetRolesAsync(user);

        return StatusCode(StatusCodes.Status201Created,
            new AuthResponseDto(token, user.Email!, user.FullName ?? "", roles));
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            return Unauthorized(new ProblemDetails
            {
                Title = "Authentication failed",
                Status = 401,
                Detail = "Invalid email or password."
            });

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
            return Unauthorized(new ProblemDetails
            {
                Title = "Authentication failed",
                Status = 401,
                Detail = result.IsLockedOut
                    ? "Account locked. Try again later."
                    : "Invalid email or password."
            });

        var token = await _tokenService.CreateTokenAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new AuthResponseDto(token, user.Email!, user.FullName ?? "", roles));
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword(
    ForgotPasswordDto dto,
    [FromServices] IEmailSender emailSender)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        // Only send the email if the user exists — but ALWAYS return 204.
        // Never tell the caller whether the email is registered.
        if (user is not null) {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // URL-encode the token because it contains characters that break URLs
            var encodedToken = System.Net.WebUtility.UrlEncode(token);

            // The frontend URL where the user lands to enter their new password
            // For learning, we'll pretend MotorsShop has a web client at this URL
            var frontendUrl = _configuration["Frontend:BaseUrl"]
                                    ?? throw new InvalidOperationException("Frontend:BaseUrl not configured.");
            var resetLink = $"{frontendUrl}/reset-password" +
                            $"?email={System.Net.WebUtility.UrlEncode(dto.Email)}" +
                            $"&token={encodedToken}";

            var body = $"""
            <p>Hi {user.FullName ?? user.Email},</p>
            <p>Someone requested a password reset for your MotorsShop account.</p>
            <p>If this was you, click the link below to set a new password:</p>
            <p><a href="{resetLink}">Reset your password</a></p>
            <p>If you didn't request this, you can ignore this email.</p>
            <p>The link expires in 1 hour.</p>
            """;

            await emailSender.SendAsync(dto.Email, "Reset your MotorsShop password", body);
        }

        return NoContent();
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            // Don't reveal whether the email exists, but ALSO don't pretend success.
            // Return a generic "invalid token" message either way.
            throw new MotorsShop.Exceptions.ValidationException(
                new Dictionary<string, string[]>
                {
                    ["Token"] = new[] { "Invalid email or token." }
                });

        var decodedToken = System.Net.WebUtility.UrlDecode(dto.Token);
        var result = await _userManager.ResetPasswordAsync(user, decodedToken, dto.NewPassword);
        if (!result.Succeeded) {
            var errors = result.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
            throw new MotorsShop.Exceptions.ValidationException(errors);
        }

        return NoContent();
    }

    [HttpPatch("password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new MotorsShop.Exceptions.NotFoundException("User not found.");

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded) {
            var errors = result.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
            throw new MotorsShop.Exceptions.ValidationException(errors);
        }

        return NoContent();
    }
}