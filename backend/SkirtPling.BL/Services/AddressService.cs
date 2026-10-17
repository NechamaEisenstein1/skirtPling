using SkirtPling.BL.DTOs;
using SkirtPling.BL.Exceptions;
using SkirtPling.BL.Interfaces;
using SkirtPling.BL.Mapping;
using SkirtPling.DAL.Entities;
using SkirtPling.DAL.Repositories;

namespace SkirtPling.BL.Services;

public class AddressService(IAddressRepository addresses) : IAddressService
{
    private const int MaxAddressesPerUser = 10;

    public async Task<IReadOnlyList<AddressDto>> GetAllAsync(string userId, CancellationToken ct) =>
        (await addresses.GetForUserAsync(userId, ct)).Select(a => a.ToDto()).ToList();

    public async Task<AddressDto> CreateAsync(string userId, AddressUpsertDto dto, CancellationToken ct)
    {
        if ((await addresses.GetForUserAsync(userId, ct)).Count >= MaxAddressesPerUser)
            throw new BusinessRuleException($"You can save up to {MaxAddressesPerUser} addresses.");

        var address = new Address { UserId = userId };
        address.Apply(dto);
        addresses.Add(address);
        await addresses.SaveChangesAsync(ct);
        return address.ToDto();
    }

    public async Task<AddressDto> UpdateAsync(string userId, int addressId, AddressUpsertDto dto, CancellationToken ct)
    {
        var address = await FindAsync(userId, addressId, ct);
        address.Apply(dto);
        await addresses.SaveChangesAsync(ct);
        return address.ToDto();
    }

    public async Task DeleteAsync(string userId, int addressId, CancellationToken ct)
    {
        var address = await FindAsync(userId, addressId, ct);
        addresses.Remove(address);
        await addresses.SaveChangesAsync(ct);
    }

    // Looking up by user AND id means one customer can never touch another customer's address.
    private async Task<Address> FindAsync(string userId, int addressId, CancellationToken ct) =>
        await addresses.GetForUserAsync(userId, addressId, ct) ?? throw new NotFoundException("Address not found.");
}
