using APBD_07.DTOs;

namespace APBD_07.Repositories;

public interface IWarehouseRepository
{
    int FulfillOrder(FulfillOrderData fulfillOrderData);
}