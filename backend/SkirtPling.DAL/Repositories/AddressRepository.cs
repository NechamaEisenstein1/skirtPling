using Microsoft.EntityFrameworkCore;
using SkirtPling.DAL.Context;
using SkirtPling.DAL.Entities;

namespace SkirtPling.DAL.Repositories;

public interface IAddressRepository
{
    Task<List<Address>> GetForUserAsync(string userId, CancellationToken ct);
    Task<Address?> GetForUserAsync(string userId, int addressId, CancellationToken ct);
    void Add(Address address);
    void Remove(Address address);
    Task SaveChangesAsync(CancellationToken ct);
}

public class AddressRepository(AppDbContext db) : IAddressRepository
{
    public Task<List<Address>> GetForUserAsync(string userId, CancellationToken ct) =>
        db.Addresses.AsNoTracking().Where(a => a.UserId == userId).OrderBy(a => a.Id).ToListAsync(ct);

    public Task<Address?> GetForUserAsync(string userId, int addressId, CancellationToken ct) =>
        db.Addresses.FirstOrDefaultAsync(a => a.UserId == userId && a.Id == addressId, ct);

    public void Add(Address address) => db.Addresses.Add(address);

    public void Remove(Address address) => db.Addresses.Remove(address);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
