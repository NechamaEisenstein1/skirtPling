using SkirtPling.DAL.Entities;

namespace SkirtPling.DAL.Repositories;

/// <summary>Search criteria for orders. UserId restricts results to one customer. Page is 1-based.</summary>
public record OrderFilter
{
    public string? UserId { get; init; }
    public OrderStatus? Status { get; init; }
    public string? Search { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
