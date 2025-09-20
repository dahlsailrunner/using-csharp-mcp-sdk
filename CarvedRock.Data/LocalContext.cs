using CarvedRock.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace CarvedRock.Data;

public class LocalContext(DbContextOptions<LocalContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; } = null!;

    [ExcludeFromCodeCoverage]
    public void MigrateAndCreateData()
    {
        Database.Migrate(); // always apply migrations       

        var pgConn = new NpgsqlConnectionStringBuilder(Database.GetConnectionString());
        if (pgConn != null && 
            !string.Equals(pgConn.Host, "localhost", StringComparison.InvariantCultureIgnoreCase) &&
            !string.Equals(pgConn.Host, "postgres", StringComparison.InvariantCultureIgnoreCase)) 
            return;

        if (Products.Any())
        {
            Products.RemoveRange(Products);
            SaveChanges();
        };


        // Can you help me generate some data for Product?  I'd like about 50 products,
        // and each of them should be something that might be found at an outdoor recreational
        // equipment store.  The categories for them should be one of  "boots", "equipment", and "kayaks".
        // Image Urls should be something from the service picsum.photos.
        // Json format is shown in the examples in this code
        string baseDirectory = AppContext.BaseDirectory;
        string jsonString = File.ReadAllText(Path.Combine(baseDirectory, "SeedData.json"));
        var products = JsonSerializer.Deserialize<List<Product>>(jsonString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (products != null)
        {
            Products.AddRange(products);
            SaveChanges();
        }

        SaveChanges();
    }
}
