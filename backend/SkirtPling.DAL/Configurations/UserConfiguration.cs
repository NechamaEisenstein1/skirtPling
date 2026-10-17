using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkirtPling.DAL.Entities;

namespace SkirtPling.DAL.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.Property(u => u.FullName).HasMaxLength(150);
        builder.HasMany(u => u.Addresses).WithOne().HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.Property(a => a.Label).HasMaxLength(50).IsRequired();
        builder.Property(a => a.FullName).HasMaxLength(150).IsRequired();
        builder.Property(a => a.Phone).HasMaxLength(30).IsRequired();
        builder.Property(a => a.Street).HasMaxLength(200).IsRequired();
        builder.Property(a => a.City).HasMaxLength(100).IsRequired();
        builder.Property(a => a.PostalCode).HasMaxLength(20).IsRequired();
        builder.Property(a => a.Country).HasMaxLength(100).IsRequired();
    }
}
