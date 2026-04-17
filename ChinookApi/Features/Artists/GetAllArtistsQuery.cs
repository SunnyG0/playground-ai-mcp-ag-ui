using MediatR;
using Microsoft.EntityFrameworkCore;
using ChinookApi.Data;

namespace ChinookApi.Features.Artists;

public record GetAllArtistsQuery(string? Search, int Page, int PageSize) : IRequest<object>;

public class GetAllArtistsHandler(ChinookContext db) : IRequestHandler<GetAllArtistsQuery, object>
{
    public async Task<object> Handle(GetAllArtistsQuery request, CancellationToken cancellationToken)
    {
        var query = db.Artists.AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(a => a.Name != null && EF.Functions.Like(a.Name, $"%{request.Search}%"));
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(a => a.Name).Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);
        return new { total, page = request.Page, pageSize = request.PageSize, items };
    }
}
