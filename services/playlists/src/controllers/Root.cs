namespace service.src.controllers;

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

[ApiController]
public class Root : Controller
{
    [HttpGet("ping")]
    [ProducesResponseType(typeof(PongResponse), StatusCodes.Status200OK)]
    public IActionResult Ping(string? echoThis)
    {
        if (echoThis is string echoThisForReal)
        {
            return Ok(new PongResponse() { Ping = $"Pong! You said {echoThisForReal}!" });
        }

        return Ok(new PongResponse() { Ping = "Pong!" });
    }
}

public record PongResponse
{
    [Required]
    public required string Ping { get; init; }
}
