using cw7.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace cw7.Endpoints;

public static class WarehouseMappings
{
    public static void MapWarehouseEndpoints(this WebApplication app)
    {
        app.MapPost("/warehouse", async ([FromServices] IWarehouseRepository db, int productId, int idWarehouse, int amount, string createdAt) =>
        {
            await db.AddProduct(productId, idWarehouse, amount, createdAt);
            return Results.Ok();
        });
    }
}