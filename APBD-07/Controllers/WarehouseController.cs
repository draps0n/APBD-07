using APBD_07.DTOs;
using APBD_07.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace APBD_07.Controllers;

[Route("/warehouse")]
[ApiController]
public class WarehouseController(IWarehouseRepository warehouseRepository) : ControllerBase
{
    private IWarehouseRepository _warehouseRepository = warehouseRepository;

    [HttpPost]
    public async Task<IActionResult> FulfillOrderAsync([FromBody] FulfillOrderData fulfillOrderData)
    {
        var productPrice = await _warehouseRepository.GetPriceOfProductByIdAsync(fulfillOrderData.IdProduct);
        if (productPrice is null) return NotFound($"Product of id: {fulfillOrderData.IdProduct} does not exist.");

        var idWarehouse = await _warehouseRepository.GetWarehouseByIdAsync(fulfillOrderData.IdWarehouse);
        Console.WriteLine($"IdWarehouse: {idWarehouse}");
        if (idWarehouse is null) return NotFound($"Warehouse of id: {fulfillOrderData.IdWarehouse} does not exist.");

        if (fulfillOrderData.Amount <= 0) return BadRequest("Amount in order cannot be <= 0.");

        var idOrder = await _warehouseRepository.GetMatchingOrderIdAsync(fulfillOrderData.IdProduct,
            fulfillOrderData.Amount,
            fulfillOrderData.CreatedAt);
        if (idOrder is null) return NotFound("No matching order found.");

        if (await _warehouseRepository.IsOrderFulfilledAsync(idOrder.Value))
        {
            return Conflict("Matching order is already fulfilled.");
        }

        var idProdWare = await _warehouseRepository.FulfillOrderAsync(
            fulfillOrderData.IdProduct,
            fulfillOrderData.IdWarehouse,
            (int)idOrder,
            fulfillOrderData.Amount,
            (decimal)productPrice,
            fulfillOrderData.CreatedAt
        );

        return Ok($"Fulfilled with index {idProdWare}");
    }
    
    [HttpPost("/warehouseProcedure")]
    public async Task<IActionResult> FulfillOrderProcAsync([FromBody] FulfillOrderData fulfillOrderData)
    {
        int idProdWare;
        try
        {
            idProdWare = await warehouseRepository.FulfillOrderProcAsync(
                fulfillOrderData.IdProduct,
                fulfillOrderData.IdWarehouse,
                fulfillOrderData.Amount,
                fulfillOrderData.CreatedAt
            );
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }

        return Ok($"Fulfilled with index {idProdWare}");
    }
}