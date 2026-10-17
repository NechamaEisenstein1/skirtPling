namespace SkirtPling.DAL.Entities;

/// <summary>
/// A server-side cart. Owned either by a signed-in user (UserId) or by a guest (GuestId, a random token kept by the browser).
/// </summary>
public class Cart
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public Guid? GuestId { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<CartItem> Items { get; set; } = [];
}
