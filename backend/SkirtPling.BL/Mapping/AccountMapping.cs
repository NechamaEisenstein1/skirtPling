using SkirtPling.BL.DTOs;
using SkirtPling.DAL.Entities;

namespace SkirtPling.BL.Mapping;

public static class AccountMapping
{
    public static AddressDto ToDto(this Address a) => new(a.Id, a.Label, a.FullName, a.Phone, a.Street, a.City, a.PostalCode, a.Country);

    public static void Apply(this Address address, AddressUpsertDto dto)
    {
        address.Label = dto.Label.Trim();
        address.FullName = dto.FullName.Trim();
        address.Phone = dto.Phone.Trim();
        address.Street = dto.Street.Trim();
        address.City = dto.City.Trim();
        address.PostalCode = dto.PostalCode.Trim();
        address.Country = dto.Country.Trim();
    }
}
