using System.Data.SqlClient;
using APBD_07.DTOs;

namespace APBD_07.Repositories;

public class WarehouseRepository(IConfiguration configuration) : IWarehouseRepository
{
    private IConfiguration _configuration = configuration;

    public int FulfillOrder(FulfillOrderData fulfillOrderData)
    {
        using var con = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        con.Open();

        // Czy produkt istnieje
        if (!DoesProductOfIdExist(con, fulfillOrderData.IdProduct))
        {
            throw new ArgumentException("Warehouse of given ID does not exist!");
        }

        // Czy magazyn istnieje
        if (!DoesWarehouseOfIdExist(con, fulfillOrderData.IdWarehouse))
        {
            throw new ArgumentException("Warehouse of given ID does not exist!");
        }

        var tmpIdOrder = GetMatchingOrderId(con, fulfillOrderData.IdProduct, fulfillOrderData.Amount);
        // Czy jest takie zamówienie
        if (tmpIdOrder is null)
        {
            throw new ArgumentException("No matching order found!");
        }

        var idOrder = (int)tmpIdOrder;

        // Czy już zrealizowane
        if (IsOrderFulfilled(con, idOrder))
        {
            throw new ArgumentException("Order already fulfilled!");
        }

        var tran = con.BeginTransaction();
        try
        {
            UpdateFulfillDate(con, tran, idOrder);
            var prodWareId = InsertNewIntoProductWarehouse(con, tran, fulfillOrderData, idOrder);
            Test(con, tran);

            tran.Rollback();
            // tran.Commit();
            return prodWareId;
        }
        catch (Exception)
        {
            tran.Rollback();
            throw;
        }
    }

    private bool DoesProductOfIdExist(SqlConnection con, int idProduct)
    {
        using var cmd = new SqlCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Product WHERE IdProduct = @IdProduct";
        cmd.Parameters.AddWithValue("@IdProduct", idProduct);
        cmd.Connection = con;

        var productOfIdCount = (int)cmd.ExecuteScalar();

        return productOfIdCount != 0;
    }

    private bool DoesWarehouseOfIdExist(SqlConnection con, int idWarehouse)
    {
        using var cmd = new SqlCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Warehouse WHERE IdWarehouse = @IdWarehouse";
        cmd.Parameters.AddWithValue("@IdWarehouse", idWarehouse);
        cmd.Connection = con;

        var warehouseOfIdCount = (int)cmd.ExecuteScalar();

        return warehouseOfIdCount != 0;
    }

    private int? GetMatchingOrderId(SqlConnection con, int idProduct, int amount)
    {
        using var cmd = new SqlCommand();
        cmd.CommandText =
            "SELECT IdOrder FROM \"Order\" WHERE IdProduct = @IdProduct AND Amount = @Amount AND FulfilledAt IS NULL;";
        cmd.Parameters.AddWithValue("@IdProduct", idProduct);
        cmd.Parameters.AddWithValue("@Amount", amount);
        cmd.Connection = con;

        var idOrder = (int?)cmd.ExecuteScalar();

        return idOrder;
    }

    private bool IsOrderFulfilled(SqlConnection con, int idOrder)
    {
        using var cmd = new SqlCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Product_Warehouse WHERE IdOrder = @IdOrder;";
        cmd.Parameters.AddWithValue("@IdOrder", idOrder);
        cmd.Connection = con;

        var fulfilledIdOrderCount = (int)cmd.ExecuteScalar();

        return fulfilledIdOrderCount != 0;
    }

    private void UpdateFulfillDate(SqlConnection con, SqlTransaction tran, int idOrder)
    {
        using var cmd = new SqlCommand();
        cmd.CommandText = "UPDATE \"Order\" SET FulfilledAt = SYSDATETIME() WHERE IdOrder = @IdOrder;";
        cmd.Parameters.AddWithValue("@IdOrder", idOrder);
        cmd.Connection = con;
        cmd.Transaction = tran;
        cmd.ExecuteNonQuery();
    }

    private int InsertNewIntoProductWarehouse(SqlConnection con, SqlTransaction tran, FulfillOrderData fulfillOrderData,
        int idOrder)
    {
        var productPrice = GetPriceOfProduct(con, tran, fulfillOrderData.IdProduct);
        using var cmd = new SqlCommand();
        cmd.CommandText =
            "INSERT INTO Product_Warehouse(IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt) VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, SYSDATETIME()); SELECT CONVERT(INT, SCOPE_IDENTITY());";
        cmd.Parameters.AddWithValue("@IdWarehouse", fulfillOrderData.IdProduct);
        cmd.Parameters.AddWithValue("@IdProduct", fulfillOrderData.IdProduct);
        cmd.Parameters.AddWithValue("@IdOrder", idOrder);
        cmd.Parameters.AddWithValue("@Amount", fulfillOrderData.Amount);
        cmd.Parameters.AddWithValue("@Price", productPrice * fulfillOrderData.Amount);
        cmd.Connection = con;
        cmd.Transaction = tran;
        return (int)(cmd.ExecuteScalar());
    }

    private Decimal GetPriceOfProduct(SqlConnection con, SqlTransaction tran, int idProduct)
    {
        using var cmd = new SqlCommand();
        cmd.CommandText =
            "SELECT Price FROM Product WHERE IdProduct = @IdProduct;";
        cmd.Parameters.AddWithValue("@IdProduct", idProduct);
        cmd.Connection = con;
        cmd.Transaction = tran;

        return (Decimal)(cmd.ExecuteScalar());
    }

    private void Test(SqlConnection con, SqlTransaction tran)
    {
        using var cmd = new SqlCommand();
        cmd.CommandText = "SELECT * FROM Product_Warehouse";
        cmd.Connection = con;
        cmd.Transaction = tran;

        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
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