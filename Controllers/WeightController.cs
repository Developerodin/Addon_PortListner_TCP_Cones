using Microsoft.AspNetCore.Mvc;
using PortListener.Services;

namespace PortListener.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeightController : ControllerBase
{
    private readonly IWeightStorageService _weightStorage;
    private readonly ILogger<WeightController> _logger;

    public WeightController(
        IWeightStorageService weightStorage,
        ILogger<WeightController> logger)
    {
        _weightStorage = weightStorage;
        _logger = logger;
    }

    [HttpGet("latest")]
    public IActionResult GetLatestWeight()
    {
        var latestWeight = _weightStorage.GetLatestWeight();
        
        if (latestWeight == null)
        {
            return NotFound(new { message = "No weight reading available yet" });
        }

        return Ok(latestWeight);
    }
}
