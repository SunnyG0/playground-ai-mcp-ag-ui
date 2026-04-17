using MediatR;
using Microsoft.EntityFrameworkCore;
using ChinookApi.Data;

namespace ChinookApi.Features.Albums;

public record GetAllAlbumsQuery(string? Search, int Page, int PageSize) : IRequest<object>;

public class GetAllAlbumsHandler(ChinookContext db) : IRequestHandler<GetAllAlbumsQuery, object>
{
    public async Task<object> Handle(GetAllAlbumsQuery request, CancellationToken cancellationToken)
    {
        var query = db.Albums.Include(a => a.Artist).AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(a => EF.Functions.Like(a.Title, $"%{request.Search}%"));
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(a => a.Title).Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);
        return new { total, page = request.Page, pageSize = request.PageSize, items };
    }
}
