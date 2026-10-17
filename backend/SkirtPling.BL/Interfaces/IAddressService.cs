using SkirtPling.BL.DTOs;

namespace SkirtPling.BL.Interfaces;

public interface IAddressService
{
    Task<IReadOnlyList<AddressDto>> GetAllAsync(string userId, CancellationToken ct);
    Task<AddressDto> CreateAsync(string userId, AddressUpsertDto dto, CancellationToken ct);
    Task<AddressDto> UpdateAsync(string userId, int addressId, AddressUpsertDto dto, CancellationToken ct);
    Task DeleteAsync(string userId, int addressId, CancellationToken ct);
}
