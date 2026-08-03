using Microsoft.EntityFrameworkCore;

using ProductApi.Data;
using ProductApi.DTOs;
using ProductApi.Extensions;
using ProductApi.Models;

namespace ProductApi.Services;

public class ProductService: IProductService
{
    private readonly AppDbContext _db;

    public ProductService(AppDbContext db)
    {
        _db = db;
    }

 public async Task<List<Product>> GetProductsAsync(
        string? search,
        decimal? minPrice,
        decimal? maxPrice,
        ProductStatus? status,
        bool? inStockOnly)
    {
        IQueryable<Product> query = _db.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(product =>
                EF.Functions.ILike(product.Name, $"%{search}%") ||
                EF.Functions.ILike(product.Description, $"%{search}%"));
        }

        if (minPrice is not null)
        {
            query = query.Where(product => product.Price >= minPrice);
        }

        if (maxPrice is not null)
        {
            query = query.Where(product => product.Price <= maxPrice);
        }

        if (status is not null)
        {
            query = query.Where(product => product.Status == status);
        }

        if (inStockOnly == true)
        {
            query = query.Where(product => product.Stocks > 0);
        }

        return await query
            .OrderBy(product => product.Name)
            .ToListAsync();
    }

    public async Task<Product?> GetProductByIdAsync(Guid id)
    {
        return await _db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(product => product.Id == id);
    }

    public async Task<Product> CreateProductAsync(CreateProductRequest request)
    {
        var product = new Product
        {
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Price = request.Price,
            Stocks = request.Stocks,
            Status = request.Status!.Value,
            CreatedDate = DateTime.UtcNow.MakeItUTC()
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        return product;
    }

public async Task<Product?> UpdateProductAsync(Guid id, UpdateProductRequest request)
    {
        var product = await _db.Products
            .FirstOrDefaultAsync(product => product.Id == id);

        if (product is null)
        {
            return null;
        }

        if (request.Name is not null)
        {
            product.Name = request.Name.Trim();
        }

        if (request.Description is not null)
        {
            product.Description = request.Description.Trim();
        }

        if (request.Price is not null)
        {
            product.Price = request.Price.Value;
        }

        if (request.Stocks is not null)
        {
            product.Stocks = request.Stocks.Value;
        }

        if (request.Status is not null)
        {
            product.Status = request.Status.Value;
        }

        await _db.SaveChangesAsync();

        return product;
    }

    public async Task<bool> DeleteProductAsync(Guid id)
    {
        var product = await _db.Products
            .FirstOrDefaultAsync(product => product.Id == id);

        if (product is null)
        {
            return false;
        }

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();

        return true;
    }

}
