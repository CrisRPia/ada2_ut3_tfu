namespace service.src.common.controllers;

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

[ApiController]
public class Root(HealthCheckService health) : Controller
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

    [HttpGet("health")]
    [ProducesResponseType(typeof(HealthCheckResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthCheckResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Health()
    {
        var report = await health.CheckHealthAsync();

        var response = new
        {
            Status = report.Status.ToString(),

            Entries = report.Entries.Select(e => new
            {
                Name = e.Key,
                Status = e.Value.Status.ToString(),
                e.Value.Description,
                Error = e.Value.Exception?.Message
            })
        };

        if (report.Status == HealthStatus.Healthy)
        {
            return Ok(response);
        }

        return StatusCode(503, response);
    }
}

public record PongResponse
{
    [Required]
    public required string Ping { get; init; }
}

public class HealthCheckResponse
{
    [Required]
    public required string Status { get; init; }
    [Required]
    public required IReadOnlyCollection<HealthCheckEntryResponse> Entries { get; init; }
}

public class HealthCheckEntryResponse
{
    [Required]
    public required string Name { get; init; }
    [Required]
    public required string Status { get; init; }
    public string? Description { get; init; }
    public string? Error { get; init; }
}

