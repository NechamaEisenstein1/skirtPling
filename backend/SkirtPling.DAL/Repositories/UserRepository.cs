using Microsoft.EntityFrameworkCore;
using SkirtPling.DAL.Context;
using SkirtPling.DAL.Entities;

namespace SkirtPling.DAL.Repositories;

/// <summary>Read-only user search for the admin area. Writes go through ASP.NET Core Identity's UserManager.</summary>
public interface IUserRepository
{
    Task<PagedList<AppUser>> SearchAsync(string? search, int page, int pageSize, CancellationToken ct);
}

public class UserRepository(AppDbContext db) : IUserRepository
{
    public async Task<PagedList<AppUser>> SearchAsync(string? search, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(u => u.Email!.Contains(term) || u.FullName.Contains(term));
        }

        var total = await query.CountAsync(ct);
        var users = await query.OrderBy(u => u.Email).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedList<AppUser>(users, total);
    }
}
