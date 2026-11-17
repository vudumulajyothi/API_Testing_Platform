using TestPilot.Domain.Enums;

namespace TestPilot.Application.DTOs.History;

public class PagedResultDto<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / (PageSize > 0 ? PageSize : 1));
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}

public class RequestHistoryDto
{
    public Guid HistoryId { get; set; }
    public Guid? RequestId { get; set; }
    public HttpMethodType Method { get; set; }
    public string Url { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public long ResponseTimeMs { get; set; }
    public string? ResponseBody { get; set; }
    public DateTime ExecutedAt { get; set; }
}
