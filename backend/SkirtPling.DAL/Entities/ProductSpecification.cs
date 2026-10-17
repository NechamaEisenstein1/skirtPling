namespace SkirtPling.DAL.Entities;

public class ProductSpecification
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
