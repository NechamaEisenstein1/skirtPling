using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkirtPling.DAL.Entities;

namespace SkirtPling.DAL.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Property(o => o.OrderNumber).HasMaxLength(30).IsRequired();
        builder.HasIndex(o => o.OrderNumber).IsUnique();
        builder.HasIndex(o => o.UserId);
        builder.Property(o => o.CustomerEmail).HasMaxLength(256).IsRequired();
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.PaymentReference).HasMaxLength(100);
        builder.Property(o => o.Subtotal).HasPrecision(10, 2);
        builder.Property(o => o.ShippingCost).HasPrecision(10, 2);
        builder.Property(o => o.Total).HasPrecision(10, 2);

        builder.OwnsOne(o => o.ShippingAddress, a =>
        {
            a.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            a.Property(x => x.Phone).HasMaxLength(30).IsRequired();
            a.Property(x => x.Street).HasMaxLength(200).IsRequired();
            a.Property(x => x.City).HasMaxLength(100).IsRequired();
            a.Property(x => x.PostalCode).HasMaxLength(20).IsRequired();
            a.Property(x => x.Country).HasMaxLength(100).IsRequired();
        });

        // Orders are kept for accounting even if the user account is removed later, so no cascade.
        builder.HasOne<AppUser>().WithMany().HasForeignKey(o => o.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(o => o.Items).WithOne().HasForeignKey(i => i.OrderId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.Property(i => i.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(i => i.Sku).HasMaxLength(40).IsRequired();
        builder.Property(i => i.UnitPrice).HasPrecision(10, 2);
        // Keep order lines even if the product is later deleted from the catalog.
        builder.HasOne<Product>().WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}
