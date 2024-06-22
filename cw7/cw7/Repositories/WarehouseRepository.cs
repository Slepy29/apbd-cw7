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

    public Task<bool> DoesMagazynExist(int id) =>
        DoesEntityExist("SELECT 1 FROM WareHouse WHERE IdWarehouse=@ID", id);

    public Task<bool> IsFulfilled(int id) =>
        DoesEntityExist("SELECT 1 FROM Product_Warehouse WHERE IdOrder=@ID", id);

    public async Task AddProduct(int id, int idWarehouse, int amount, string createdAt)
    {
        var query = @"
            INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
            VALUES (@IdWarehouse, @IdProduct, 1, @Amount, (SELECT Price FROM Product WHERE IdProduct=@IdProduct), @CreatedAt);
            SELECT SCOPE_IDENTITY();";
        
        idCounter++;

        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@IdWarehouse", idWarehouse);
        command.Parameters.AddWithValue("@IdProduct", id);
        command.Parameters.AddWithValue("@Amount", amount);
        command.Parameters.AddWithValue("@CreatedAt", createdAt);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
    }

    public Task<bool> IsInOrder(int id) =>
        DoesEntityExist("SELECT 1 FROM [Order] WHERE IdProduct=@ID", id);

    public async Task UpdateData(int id)
    {
        var query = "UPDATE [Order] SET FulfilledAt = GETDATE() WHERE IdProduct=@ID";

        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }
}