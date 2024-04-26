using System.Data;
using System.Data.SqlClient;

namespace APBD_07.Repositories;

public class WarehouseRepository(IConfiguration configuration) : IWarehouseRepository
{
    private IConfiguration _configuration = configuration;

    public async Task<decimal?> GetPriceOfProductByIdAsync(int idProduct)
    {
        await using var con = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await con.OpenAsync();
        await using var cmd = new SqlCommand();
        cmd.CommandText = "SELECT p.Price FROM Product p WHERE IdProduct = @IdProduct";
        cmd.Parameters.AddWithValue("@IdProduct", idProduct);
        cmd.Connection = con;
        return (decimal?)await cmd.ExecuteScalarAsync();
    }

    public async Task<int?> GetWarehouseByIdAsync(int idWarehouse)
    {
        await using var con = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await con.OpenAsync();
        await using var cmd = new SqlCommand();
        cmd.CommandText = "SELECT IdWarehouse FROM Warehouse WHERE IdWarehouse = @IdWarehouse";
        cmd.Parameters.AddWithValue("@IdWarehouse", idWarehouse);
        cmd.Connection = con;
        return (int?)await cmd.ExecuteScalarAsync();
    }

    public async Task<int?> GetMatchingOrderIdAsync(int idProduct, int amount, DateTime createdAt)
    {
        await using var con = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await con.OpenAsync();
        await using var cmd = new SqlCommand();
        cmd.CommandText =
            "SELECT IdOrder FROM [Order] WHERE IdProduct = @IdProduct AND Amount = @Amount AND CreatedAt < @CreatedAt;";
        cmd.Parameters.AddWithValue("@IdProduct", idProduct);
        cmd.Parameters.AddWithValue("@Amount", amount);
        cmd.Parameters.AddWithValue("@CreatedAt", createdAt);
        cmd.Connection = con;
        return (int?)await cmd.ExecuteScalarAsync();
    }

    public async Task<bool> IsOrderFulfilledAsync(int idOrder)
    {
        await using var con = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await con.OpenAsync();
        await using var cmd = new SqlCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Product_Warehouse WHERE IdOrder = @IdOrder;";
        cmd.Parameters.AddWithValue("@IdOrder", idOrder);
        cmd.Connection = con;

        var fulfilledIdOrderCount = (int?)await cmd.ExecuteScalarAsync();

        return fulfilledIdOrderCount != 0;
    }

    public async Task<int> FulfillOrderAsync(int idProduct, int idWarehouse, int idOrder, int amount,
        decimal productPrice, DateTime createdAt)
    {
        await using var con = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await con.OpenAsync();
        var tran = await con.BeginTransactionAsync();
        try
        {
            await using var updateCmd = new SqlCommand();
            updateCmd.CommandText = "UPDATE [Order] SET FulfilledAt = SYSDATETIME() WHERE IdOrder = @IdOrder;";
            updateCmd.Parameters.AddWithValue("@IdOrder", idOrder);
            updateCmd.Connection = con;
            updateCmd.Transaction = (SqlTransaction)tran;
            await updateCmd.ExecuteNonQueryAsync();

            await using var insertCmd = new SqlCommand();
            insertCmd.CommandText =
                "INSERT INTO Product_Warehouse(IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt) " +
                "VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, SYSDATETIME()); " +
                "SELECT CONVERT(INT, SCOPE_IDENTITY());";
            insertCmd.Parameters.AddWithValue("@IdWarehouse", idWarehouse);
            insertCmd.Parameters.AddWithValue("@IdProduct", idProduct);
            insertCmd.Parameters.AddWithValue("@IdOrder", idOrder);
            insertCmd.Parameters.AddWithValue("@Amount", amount);
            insertCmd.Parameters.AddWithValue("@Price", productPrice * amount);
            insertCmd.Connection = con;
            insertCmd.Transaction = (SqlTransaction)tran;
            var prodWareId = (int?)await insertCmd.ExecuteScalarAsync();
            await tran.CommitAsync();
            return (int)prodWareId!;
        }
        catch (Exception)
        {
            await tran.RollbackAsync();
            throw;
        }
    }
    
    public async Task<int> FulfillOrderProcAsync(int idProduct, int idWarehouse, int amount, DateTime createdAt)
    {
        await using var con = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await con.OpenAsync();
        await using var cmd = new SqlCommand();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "AddProductToWarehouse";
        cmd.Connection = con;
        cmd.Parameters.AddWithValue("@IdProduct", idProduct);
        cmd.Parameters.AddWithValue("@IdWarehouse", idWarehouse);
        cmd.Parameters.AddWithValue("@Amount", amount);
        cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
        var prodWareId = (int)(await cmd.ExecuteScalarAsync())!;
        return prodWareId;
    }
}