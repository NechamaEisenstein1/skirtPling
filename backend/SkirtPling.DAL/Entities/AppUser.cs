using Microsoft.AspNetCore.Identity;

namespace SkirtPling.DAL.Entities;

public class AppUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<Address> Addresses { get; set; } = [];
}
