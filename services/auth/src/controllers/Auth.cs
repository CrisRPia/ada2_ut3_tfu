using System.ComponentModel.DataAnnotations;
using service.src.models;
using service.src.services;
using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace service.src.controllers;

[ApiController]
[Produces("application/json")]
public class AuthController(ApplicationDbContext context, JwtTokenGenerator tokenGenerator)
    : Controller
{
    /// <summary>
    /// Registers a new user and returns a JWT upon success.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (await context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return Conflict("User with this email already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = Argon2.Hash(request.MasterPassword),
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var token = tokenGenerator.GenerateToken(user);
        return Ok(new TokenResponse { Token = token });
    }

    /// <summary>
    /// Logs in an existing user and returns a JWT upon success.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !Argon2.Verify(user.PasswordHash, request.MasterPassword))
        {
            return Unauthorized("Invalid credentials.");
        }

        var token = tokenGenerator.GenerateToken(user);
        return Ok(new TokenResponse { Token = token });
    }
}

// --- DTOs ---

public record RegisterRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    [MinLength(8)]
    public required string MasterPassword { get; init; }
}

public record LoginRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required string MasterPassword { get; init; }
}

public record TokenResponse
{
    [Required]
    public required string Token { get; init; }
}
