using MediatR;
using Microsoft.EntityFrameworkCore;
using ChinookApi.Data;
using ChinookApi.Features.Common;
using ChinookApi.Models;

namespace ChinookApi.Features.Tracks;

public record GetAllTracksQuery(string? Search, int? GenreId, int Page, int PageSize) : IRequest<PagedResult<Track>>;

public class GetAllTracksHandler(ChinookContext db) : IRequestHandler<GetAllTracksQuery, PagedResult<Track>>
{
    public async Task<PagedResult<Track>> Handle(GetAllTracksQuery request, CancellationToken cancellationToken)
    {
        var query = db.Tracks.Include(t => t.Album).Include(t => t.Genre).AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(t => EF.Functions.Like(t.Name, $"%{request.Search}%"));
        if (request.GenreId.HasValue)
            query = query.Where(t => t.GenreId == request.GenreId);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(t => t.Name).Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);
        return new PagedResult<Track>(total, request.Page, request.PageSize, items);
    }
}
