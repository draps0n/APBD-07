using System.Data.SqlClient;
using APBD_07.DTOs;

namespace APBD_07.Repositories;

public class WarehouseRepository(IConfiguration configuration) : IWarehouseRepository
{
    private IConfiguration _configuration = configuration;

    public async Task<int> FulfillOrderAsync(FulfillOrderData fulfillOrderData)
    {
        await using var con = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await con.OpenAsync();

        // Czy produkt istnieje
        if (!await DoesProductOfIdExistAsync(con, fulfillOrderData.IdProduct))
        {
            throw new ArgumentException("Warehouse of given ID does not exist!");
        }

        // Czy magazyn istnieje
        if (!await DoesWarehouseOfIdExistAsync(con, fulfillOrderData.IdWarehouse))
        {
            throw new ArgumentException("Warehouse of given ID does not exist!");
        }

        var tmpIdOrder = await GetMatchingOrderIdAsync(con, fulfillOrderData.IdProduct, fulfillOrderData.Amount);
        // Czy jest takie zamówienie
        if (tmpIdOrder is null)
        {
            throw new ArgumentException("No matching order found!");
        }

        var idOrder = (int)tmpIdOrder;

        // Czy już zrealizowane
        if (await IsOrderFulfilledAsync(con, idOrder))
        {
            throw new ArgumentException("Order already fulfilled!");
        }

        var tran = await con.BeginTransactionAsync();
        try
        {
            await UpdateFulfillDateAsync(con, (SqlTransaction)tran, idOrder);
            var prodWareId =
                await InsertNewIntoProductWarehouseAsync(con, (SqlTransaction)tran, fulfillOrderData, idOrder);

            await TestAsync(con, (SqlTransaction)tran);
            await tran.RollbackAsync();

            // await tran.CommitAsync();
            return (int) prodWareId!;
        }
        catch (Exception)
        {
            await tran.RollbackAsync();
            throw;
        }
    }

    private async Task<bool> DoesProductOfIdExistAsync(SqlConnection con, int idProduct)
    {
        await using var cmd = new SqlCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Product WHERE IdProduct = @IdProduct";
        cmd.Parameters.AddWithValue("@IdProduct", idProduct);
        cmd.Connection = con;

        var productOfIdCount = (int?) await cmd.ExecuteScalarAsync();

        return productOfIdCount != 0;
    }

    private async Task<bool> DoesWarehouseOfIdExistAsync(SqlConnection con, int idWarehouse)
    {
        await using var cmd = new SqlCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Warehouse WHERE IdWarehouse = @IdWarehouse";
        cmd.Parameters.AddWithValue("@IdWarehouse", idWarehouse);
        cmd.Connection = con;

        var warehouseOfIdCount = (int?) await cmd.ExecuteScalarAsync();

        return warehouseOfIdCount != 0;
    }

    private async Task<int?> GetMatchingOrderIdAsync(SqlConnection con, int idProduct, int amount)
    {
        await using var cmd = new SqlCommand();
        cmd.CommandText =
            "SELECT IdOrder FROM \"Order\" WHERE IdProduct = @IdProduct AND Amount = @Amount AND FulfilledAt IS NULL;";
        cmd.Parameters.AddWithValue("@IdProduct", idProduct);
        cmd.Parameters.AddWithValue("@Amount", amount);
        cmd.Connection = con;

        var idOrder = (int?) await cmd.ExecuteScalarAsync();

        return idOrder;
    }

    private async Task<bool> IsOrderFulfilledAsync(SqlConnection con, int idOrder)
    {
        await using var cmd = new SqlCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Product_Warehouse WHERE IdOrder = @IdOrder;";
        cmd.Parameters.AddWithValue("@IdOrder", idOrder);
        cmd.Connection = con;

        var fulfilledIdOrderCount = (int?) await cmd.ExecuteScalarAsync();

        return fulfilledIdOrderCount != 0;
    }

    private async Task UpdateFulfillDateAsync(SqlConnection con, SqlTransaction tran, int idOrder)
    {
        await using var cmd = new SqlCommand();
        cmd.CommandText = "UPDATE \"Order\" SET FulfilledAt = SYSDATETIME() WHERE IdOrder = @IdOrder;";
        cmd.Parameters.AddWithValue("@IdOrder", idOrder);
        cmd.Connection = con;
        cmd.Transaction = tran;
        await cmd.ExecuteNonQueryAsync();
    }

    private async Task<int?> InsertNewIntoProductWarehouseAsync(SqlConnection con, SqlTransaction tran,
        FulfillOrderData fulfillOrderData,
        int idOrder)
    {
        var productPrice = await GetPriceOfProductAsync(con, tran, fulfillOrderData.IdProduct);
        await using var cmd = new SqlCommand();
        cmd.CommandText =
            "INSERT INTO Product_Warehouse(IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt) VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, SYSDATETIME()); SELECT CONVERT(INT, SCOPE_IDENTITY());";
        cmd.Parameters.AddWithValue("@IdWarehouse", fulfillOrderData.IdProduct);
        cmd.Parameters.AddWithValue("@IdProduct", fulfillOrderData.IdProduct);
        cmd.Parameters.AddWithValue("@IdOrder", idOrder);
        cmd.Parameters.AddWithValue("@Amount", fulfillOrderData.Amount);
        cmd.Parameters.AddWithValue("@Price", productPrice * fulfillOrderData.Amount);
        cmd.Connection = con;
        cmd.Transaction = tran;
        
        var res = (int?)await cmd.ExecuteScalarAsync();
        return res;
    }

    private async Task<decimal?> GetPriceOfProductAsync(SqlConnection con, SqlTransaction tran, int idProduct)
    {
        await using var cmd = new SqlCommand();
        cmd.CommandText =
            "SELECT Price FROM Product WHERE IdProduct = @IdProduct;";
        cmd.Parameters.AddWithValue("@IdProduct", idProduct);
        cmd.Connection = con;
        cmd.Transaction = tran;

        var res = (decimal?) await cmd.ExecuteScalarAsync();
        return res;
    }

    private async Task TestAsync(SqlConnection con, SqlTransaction tran)
    {
        await using var cmd = new SqlCommand();
        cmd.CommandText = "SELECT * FROM Product_Warehouse";
        cmd.Connection = con;
        cmd.Transaction = tran;

        await using var reader = await cmd.ExecuteReaderAsync();
        while (reader.Read())
        {
            Console.WriteLine("Czytam");
            var idProdWare = reader.GetInt32(0);
            var idWare = reader.GetInt32(1);
            var idProd = reader.GetInt32(2);
            var idOrder = reader.GetInt32(3);
            var amount = reader.GetInt32(4);
            var price = reader.GetDecimal(5);
            var createdAt = reader.GetDateTime(6);

            Console.WriteLine(
                idProdWare + "\t" +
                idWare + "\t" +
                idProd + "\t" +
                idOrder + "\t" +
                amount + "\t" +
                price + "\t" +
                createdAt
            );
        }
    }
}