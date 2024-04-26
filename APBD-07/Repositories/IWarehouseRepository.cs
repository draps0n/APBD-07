using APBD_07.DTOs;

namespace APBD_07.Repositories;

public interface IWarehouseRepository
{
    Task<int> FulfillOrderAsync(int idProduct, int idWarehouse, int idOrder, int amount,
        decimal productPrice, DateTime createdAt);
    Task<int> FulfillOrderProcAsync(int idProduct, int idWarehouse, int amount, DateTime createdAt);
    Task<decimal?> GetPriceOfProductByIdAsync(int idProduct);
    Task<int?> GetWarehouseByIdAsync(int idWarehouse);
    Task<int?> GetMatchingOrderIdAsync(int idProduct, int amount, DateTime createdAt);
    Task<bool> IsOrderFulfilledAsync(int idOrder);
}