namespace ChinookApi.Features.Common;

public record PagedResult<T>(int Total, int Page, int PageSize, List<T> Items);
