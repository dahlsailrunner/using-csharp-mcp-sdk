using CarvedRock.Core;
using CarvedRock.Data.Entities;
using Riok.Mapperly.Abstractions;

namespace CarvedRock.Domain.Mapping;

[Mapper]
public partial class ProductMapper
{
    public partial ProductModel ProductToProductModel(Product product);
    public partial Product ProductModelToProduct(ProductModel productModel);

    [MapperIgnoreTarget(nameof(Product.Id))]
    public partial Product NewProductModelToProduct(NewProductModel newProductModel);
}
