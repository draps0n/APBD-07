using System.ComponentModel.DataAnnotations;

namespace APBD_07.DTOs;

public record FulfillOrderData(
    [Required] int IdProduct,
    [Required] int IdWarehouse,
    [Required] int Amount,
    [Required] DateTime CreatedAt
);