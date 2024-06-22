namespace cw7.Repositories;

public interface IWarehouseRepository
{
    public Task<bool> DoesEntityExist(string query, int id);

    public Task<bool> DoesProductExist(int id);

    public Task<bool> DoesWarehouseExist(int id);

    public Task<(bool, decimal, DateTime)> CheckOrderExists(int productId, int amount, DateTime createdAt);

    public Task<bool> IsOrderFulfilled(int idOrder);

    public Task UpdateOrderFulfilledAt(int productId);

    public Task<int> AddProductToWarehouse(int productId, int warehouseId, int amount, string createdAt);

}