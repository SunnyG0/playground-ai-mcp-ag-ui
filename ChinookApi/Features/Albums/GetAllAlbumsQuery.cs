using MediatR;
using Microsoft.EntityFrameworkCore;
using ChinookApi.Data;
using ChinookApi.Features.Common;
using ChinookApi.Models;

namespace ChinookApi.Features.Albums;

public record GetAllAlbumsQuery(string? Search, int Page, int PageSize) : IRequest<PagedResult<Album>>;

public class GetAllAlbumsHandler(ChinookContext db) : IRequestHandler<GetAllAlbumsQuery, PagedResult<Album>>
{
    public async Task<PagedResult<Album>> Handle(GetAllAlbumsQuery request, CancellationToken cancellationToken)
    {
        var query = db.Albums.Include(a => a.Artist).AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(a => EF.Functions.Like(a.Title, $"%{request.Search}%"));
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(a => a.Title).Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);
        return new PagedResult<Album>(total, request.Page, request.PageSize, items);
    }
}
