
using apbd_cw7.Exceptions;
using apbd_cw7.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace apbd_cw7.Controllers;

[ApiController]
[Route("[controller]")]
public class ClientController(IDbService service) : ControllerBase
{ 
    [HttpGet("/clients/{id}/trips")]
public async Task<IActionResult> GetAllTripsForClient(int id)
{
    try
    {
        var trips = await service.GetTripByIdAsync(id);
        return Ok(trips);
    }
    catch (NotFoundException ex)
    {
        return NotFound(ex.Message);
    }
}

    [HttpPost("/clients")]
    public async Task<IActionResult> CreateClient([FromBody] ClientCreateDTO client)
    {
        var result = await service.CreatClientByIdAsync(client);
        return CreatedAtAction(nameof(GetAllClients), new {id = result.IdClient}, result);
    }

    [HttpGet("/clients")]
    public async Task<IActionResult> GetAllClients()
    {
        return Ok(await service.GetAllClientsAsync());
    }

}