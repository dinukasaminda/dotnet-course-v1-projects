using ProductApi.DTOs;
using ProductApi.Models;

namespace ProductApi.Services;

public interface IProductService
{
    //Get products
    Task<List<Product>> GetProductsAsync(
        string? search,
        decimal? minPrice,
        decimal? maxPrice,
        ProductStatus? status,
        bool? isStockOnly
    );

    // Get product by id 
    Task<Product?> GetProductByIdAsync(Guid id);

    // Create product
    Task<Product> CreateProductAsync(CreateProductRequest request);
    
    // Update producct 
    Task<Product?> UpdateProductAsync( Guid id, UpdateProductRequest request);

    // delete product 
    Task<bool> DeleteProductAsync(Guid id);

}