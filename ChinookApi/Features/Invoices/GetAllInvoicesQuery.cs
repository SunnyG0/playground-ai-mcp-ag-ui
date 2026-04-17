using MediatR;
using Microsoft.EntityFrameworkCore;
using ChinookApi.Data;

namespace ChinookApi.Features.Invoices;

public record GetAllInvoicesQuery(int Page, int PageSize) : IRequest<object>;

public class GetAllInvoicesHandler(ChinookContext db) : IRequestHandler<GetAllInvoicesQuery, object>
{
    public async Task<object> Handle(GetAllInvoicesQuery request, CancellationToken cancellationToken)
    {
        var query = db.Invoices.Include(i => i.Customer);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(i => i.InvoiceDate).Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);
        return new { total, page = request.Page, pageSize = request.PageSize, items };
    }
}
