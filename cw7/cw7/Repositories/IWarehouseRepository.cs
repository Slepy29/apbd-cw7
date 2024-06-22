namespace cw7.Repositories;

public interface IWarehouseRepository
{
    public Task<bool> DoesEntityExist(string query, int id);

    public Task<bool> DoesProductExist(int id);

    public Task<bool> DoesMagazynExist(int id);

    public Task<bool> IsFulfilled(int id);

    public Task AddProduct(int id, int idWarehouse, int amount, string createdAt);

    public Task<bool> IsInOrder(int id);

    public Task UpdateData(int id);

}