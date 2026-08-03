using Microsoft.EntityFrameworkCore;
using ProductApi.Extensions;
using ProductApi.Models;

namespace ProductApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
    {
        
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p =>p.Id);
            entity.Property(p=>p.Id)
            .HasColumnType("uuid");

            entity.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

            entity.Property(p => p.Description)
            .HasMaxLength(500);

            entity.Property(p => p.Price)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

            entity.Property(p => p.Status)
            .HasConversion( 
                status => status.ToString().ToUpper(),
                value => Enum.Parse<ProductStatus>(value, ignoreCase: true)
            );

            entity.Property(p => p.CreatedDate)
            .HasConversion( 
                dt => dt.MakeItUTC(),
                dt => dt.MakeItUTC()
            )
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        });
    }


}