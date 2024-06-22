using Microsoft.Data.SqlClient;

namespace cw7.Repositories;

public class WarehouseRepository : IWarehouseRepository
{
    private static int idCounter = 0;
    private readonly IConfiguration _configuration;
    public WarehouseRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task<bool> DoesEntityExist(string query, int id)
    {
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return result != null;
    }

    public Task<bool> DoesProductExist(int id) =>
        DoesEntityExist("SELECT 1 FROM Product WHERE IdProduct=@ID", id);

    public Task<bool> DoesWarehouseExist(int id) =>
        DoesEntityExist("SELECT 1 FROM WareHouse WHERE IdWarehouse=@ID", id);

    public async Task<(bool, decimal, DateTime)> CheckOrderExists(int productId, int amount, DateTime createdAt)
    {
        var query = @"
            SELECT Amount, CreatedAt
            FROM [Order]
            WHERE IdProduct = @ID AND Amount = @Amount AND CreatedAt < @CreatedAt";

        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ID", productId);
        command.Parameters.AddWithValue("@Amount", amount);
        command.Parameters.AddWithValue("@CreatedAt", createdAt);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return (true, reader.GetDecimal(0), reader.GetDateTime(1));
        }

        return (false, 0, DateTime.MinValue);
    }

    public Task<bool> IsOrderFulfilled(int idOrder) =>
        DoesEntityExist("SELECT 1 FROM Product_Warehouse WHERE IdOrder=@ID", idOrder);

    public async Task UpdateOrderFulfilledAt(int productId)
    {
        var query = "UPDATE [Order] SET FulfilledAt = GETDATE() WHERE IdProduct=@ID";

        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ID", productId);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task<int> AddProductToWarehouse(int productId, int warehouseId, int amount, string createdAt)
    {
        if (amount <= 0.2M)
        {
            throw new ArgumentException("Amount must be greater than 0.2.");
        }

        if (!await DoesProductExist(productId))
        {
            throw new InvalidOperationException("Product does not exist.");
        }

        if (!await DoesWarehouseExist(warehouseId))
        {
            throw new InvalidOperationException("Warehouse does not exist.");
        }

        var (orderExists, orderAmount, orderCreatedAt) = await CheckOrderExists(productId, amount, DateTime.Parse(createdAt));
        if (!orderExists)
        {
            throw new InvalidOperationException("Matching order does not exist.");
        }

        if (await IsOrderFulfilled(productId))
        {
            throw new InvalidOperationException("Order has already been fulfilled.");
        }

        await UpdateOrderFulfilledAt(productId);

        var query = @"
            INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
            VALUES (@IdWarehouse, @IdProduct, 1, @Amount, (SELECT Price FROM Product WHERE IdProduct=@IdProduct) * @Amount, @CreatedAt);
            SELECT SCOPE_IDENTITY();";

        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@IdWarehouse", warehouseId);
        command.Parameters.AddWithValue("@IdProduct", productId);
        command.Parameters.AddWithValue("@Amount", amount);
        command.Parameters.AddWithValue("@CreatedAt", createdAt);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }
}