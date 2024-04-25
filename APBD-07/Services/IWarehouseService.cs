using APBD_07.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace APBD_07.Services;

public interface IWarehouseService
{
    public Task<int> FulfillOrderAsync(FulfillOrderData fulfillOrderData);
    public Task<int> FulfillOrderProcAsync(FulfillOrderData fulfillOrderData);
}