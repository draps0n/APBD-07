using APBD_07.DTOs;
using APBD_07.Repositories;

namespace APBD_07.Services;

public class WarehouseService(IWarehouseRepository warehouseRepository) : IWarehouseService
{

    private IWarehouseRepository _warehouseRepository = warehouseRepository;
    
    public async Task<int> FulfillOrderAsync(FulfillOrderData fulfillOrderData)
    {
        if (fulfillOrderData.Amount <= 0)
        {
            throw new ArgumentException("Amount cannot be less equal than 0!");
        }

        return await warehouseRepository.FulfillOrderAsync(fulfillOrderData);
    }

    public async Task<int> FulfillOrderProcAsync(FulfillOrderData fulfillOrderData)
    {
        if (fulfillOrderData.Amount <= 0)
        {
            throw new ArgumentException("Amount cannot be less equal than 0!");
        }

        return await warehouseRepository.FulfillOrderProcAsync(fulfillOrderData);
    }
}