namespace SkirtPling.DAL.Entities;

/// <summary>A copy of the delivery address stored on the order, so later profile edits don't change past orders.</summary>
public class ShippingAddress
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}
