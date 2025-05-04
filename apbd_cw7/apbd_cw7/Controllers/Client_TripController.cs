using apbd_cw7.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace apbd_cw7.Controllers;
[ApiController]
[Route("[controller]")]
public class Client_TripController(IDbService service) : ControllerBase
{
    [HttpPut("/clients/{id}/trips/{tripId}")]
    public async Task<IActionResult> UpdateTrip(int id, int tripId)
    {
        try
        {
            var result = await service.GetClientTripByIdsAsync(id, tripId);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("/clients/{id}/trips/{tripId}")]
    public async Task<IActionResult> DeleteTrip(int id, int tripId)
    {
        try
        {
            await service.DeleteClientTripByIdsAsync(id, tripId);
            return Ok($"Client with id {id} has been successfully removed from trip {tripId}.");
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}