using APBD_07.DTOs;
using APBD_07.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD_07.Controllers;

[Route("/warehouseProc")]
[ApiController]
public class WarehouseProcController(IWarehouseService warehouseService) : ControllerBase
{
    private IWarehouseService _warehouseService = warehouseService;
    
    [HttpPost]
    public async Task<IActionResult> FulfillOrderProcAsync([FromBody] FulfillOrderData fulfillOrderData)
    {
        int id;
        try
        {
            id = await _warehouseService.FulfillOrderProcAsync(fulfillOrderData);
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
        return Ok(id);
    }
}