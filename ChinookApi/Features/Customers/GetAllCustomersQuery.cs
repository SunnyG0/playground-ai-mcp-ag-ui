using MediatR;
using Microsoft.EntityFrameworkCore;
using ChinookApi.Data;

namespace ChinookApi.Features.Customers;

public record GetAllCustomersQuery(string? Search, int Page, int PageSize) : IRequest<object>;

public class GetAllCustomersHandler(ChinookContext db) : IRequestHandler<GetAllCustomersQuery, object>
{
    public async Task<object> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        var query = db.Customers.AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(c => EF.Functions.Like(c.FirstName + " " + c.LastName, $"%{request.Search}%") || EF.Functions.Like(c.Email, $"%{request.Search}%"));
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(c => c.LastName).ThenBy(c => c.FirstName).Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);
        return new { total, page = request.Page, pageSize = request.PageSize, items };
    }
}
