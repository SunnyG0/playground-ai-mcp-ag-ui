using MediatR;
using Microsoft.EntityFrameworkCore;
using ChinookApi.Data;
using ChinookApi.Features.Common;
using ChinookApi.Models;

namespace ChinookApi.Features.Artists;

public record GetAllArtistsQuery(string? Search, int Page, int PageSize) : IRequest<PagedResult<Artist>>;

public class GetAllArtistsHandler(ChinookContext db) : IRequestHandler<GetAllArtistsQuery, PagedResult<Artist>>
{
    public async Task<PagedResult<Artist>> Handle(GetAllArtistsQuery request, CancellationToken cancellationToken)
    {
        var query = db.Artists.AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(a => a.Name != null && EF.Functions.Like(a.Name, $"%{request.Search}%"));
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(a => a.Name).Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);
        return new PagedResult<Artist>(total, request.Page, request.PageSize, items);
    }
}
