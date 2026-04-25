namespace Ambev.DeveloperEvaluation.Domain.Common;

public class PaginatedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int CurrentPage { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    public PaginatedResult(IEnumerable<T> items, int currentPage, int pageSize, int totalCount)
    {
        Items = items.ToList().AsReadOnly();
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalCount = totalCount;
    }
}
