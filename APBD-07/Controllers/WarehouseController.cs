using APBD_07.DTOs;
using APBD_07.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD_07.Controllers;

[Route("/warehouse")]
[ApiController]
public class WarehouseController(IWarehouseService warehouseService) : ControllerBase
{
    private IWarehouseService _warehouseService = warehouseService;
    
    [HttpPost]
    public async Task<IActionResult> FulfillOrderAsync([FromBody] FulfillOrderData fulfillOrderData)
    {
        int id;
        try
        {
            id = await _warehouseService.FulfillOrder(fulfillOrderData);
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
        return Ok(id);
    }
}