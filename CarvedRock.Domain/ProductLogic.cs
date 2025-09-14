using CarvedRock.Core;
using CarvedRock.Data;
using CarvedRock.Data.Entities;
using CarvedRock.Domain.Mapping;
using Microsoft.Extensions.Logging;

namespace CarvedRock.Domain;

public class ProductLogic(ILogger<ProductLogic> logger, ICarvedRockRepository repo) : IProductLogic
{
    public async Task<IEnumerable<Product>> GetProductsForCategoryAsync(string category)
    {               
        logger.LogInformation("Getting products in logic for {category}", category);
        return await repo.GetProductsAsync(category);
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await repo.GetProductByIdAsync(id);
    }        

    public async Task<ProductModel> CreateProductAsync(NewProductModel newProduct)
    {       
        var productMapper = new ProductMapper();

        var productToCreate = productMapper.NewProductModelToProduct(newProduct);
        var createdProduct = await repo.CreateProductAsync(productToCreate);
        return productMapper.ProductToProductModel(createdProduct);
    }
}