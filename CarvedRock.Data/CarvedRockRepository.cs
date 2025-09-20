using CarvedRock.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CarvedRock.Data;

public class CarvedRockRepository(LocalContext ctx, ILogger<CarvedRockRepository> logger) : ICarvedRockRepository
{
    public async Task<List<Product>> GetProductsAsync(string category)
    {          
        logger.LogInformation("Getting products in repository for {category}", category);

        List<string> validCategories = ["kayak", "equip", "boots", "all"];

        try
        {
            if (!validCategories.Contains(category))
            {
                throw new Exception($"Simulated exception for category {category}");
            }

            return await ctx.Products.Where(p => p.Category == category || category == "all")
                .OrderBy(p => p.Id)
                .ToListAsync();
        } 
        catch (Exception ex)
        {
            var newEx = new ApplicationException("Something bad happened in database", ex);
            newEx.Data.Add("Category", category);
            throw newEx;
        }
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await ctx.Products.FindAsync(id);
    }

    public Task<bool> IsProductNameUniqueAsync(string name)
    {
        return ctx.Products.AllAsync(p => p.Name != name);
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        product.Name = product.Name!.Trim();
        ctx.Products.Add(product);
        await ctx.SaveChangesAsync();
        return product;
    }
    
    public async Task<Product> UpdateProductAsync(Product product)
    {
        var existingProduct = await ctx.Products.FindAsync(product.Id) 
            ?? throw new KeyNotFoundException($"Product with ID {product.Id} not found");
            
        existingProduct.Name = product.Name!.Trim();
        existingProduct.Description = product.Description;
        existingProduct.Price = product.Price;
        existingProduct.Category = product.Category;
        existingProduct.ImgUrl = product.ImgUrl;
        
        await ctx.SaveChangesAsync();
        return existingProduct;
    }
    
    public async Task DeleteProductAsync(int id)
    {
        var product = await ctx.Products.FindAsync(id) 
            ?? throw new KeyNotFoundException($"Product with ID {id} not found");
            
        ctx.Products.Remove(product);
        await ctx.SaveChangesAsync();
    }
}
