namespace LMS.Infrastructure.Common;

/// <summary>Generic paged response wrapper returned by list/search endpoints.</summary>
public sealed class PagedResult<T>
{
    public List<T> Items { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}
