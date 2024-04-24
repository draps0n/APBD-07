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
    public IActionResult FulfillOrder([FromBody] FulfillOrderData fulfillOrderData)
    {
        var id = -1;
        try
        {
            id = _warehouseService.FulfillOrder(fulfillOrderData);
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
        return Ok(id);
    }
}