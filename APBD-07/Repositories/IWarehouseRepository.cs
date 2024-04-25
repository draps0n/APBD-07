using APBD_07.DTOs;

namespace APBD_07.Repositories;

public interface IWarehouseRepository
{
    Task<int> FulfillOrderAsync(FulfillOrderData fulfillOrderData);
    Task<int> FulfillOrderProcAsync(FulfillOrderData fulfillOrderData);
}