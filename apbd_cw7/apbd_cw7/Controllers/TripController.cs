using Microsoft.AspNetCore.Mvc;

namespace apbd_cw7.Controllers;

[ApiController]
[Route("[controller]")]
public class TripController(IDbService service) : ControllerBase
{
    [HttpGet("/trips")]
    public async Task<IActionResult> GetAllTrips()
    {
        return Ok(await service.GetAllTripsAsync());
    }
}