using APBD_07.DTOs;

namespace APBD_07.Services;

public interface IWarehouseService
{
    int FulfillOrder(FulfillOrderData fulfillOrderData);
}