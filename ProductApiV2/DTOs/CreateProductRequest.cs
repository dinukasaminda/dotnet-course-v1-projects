using ProductApi.Models;

namespace ProductApi.DTOs;

public class CreateProductRequest
{
    public string Name {get; set;} = string.Empty;
    public string Description {get; set;} = string.Empty;
    public decimal Price {get; set;} 

    public int Stocks {get; set;}

    public ProductStatus? Status { get; set;}

}