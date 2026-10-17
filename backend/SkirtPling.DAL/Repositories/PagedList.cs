namespace SkirtPling.DAL.Repositories;

/// <summary>One page of query results plus the total number of matching rows.</summary>
public record PagedList<T>(IReadOnlyList<T> Items, int TotalCount);
