namespace ProductApi.Models;


public class Product
{
    public Guid Id{ get; set; } = Guid.NewGuid();
    public string Name {get; set;} = string.Empty;
    public string Description {get; set;} = string.Empty;

    public decimal Price {get; set;}

    public int Stocks {get; set;}

    public  ProductStatus Status {get; set;} = ProductStatus.Draft;

    public DateTime CreatedDate {get; set;} = DateTime.UtcNow;

}