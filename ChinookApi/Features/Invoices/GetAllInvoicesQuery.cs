using MediatR;
using Microsoft.EntityFrameworkCore;
using ChinookApi.Data;
using ChinookApi.Features.Common;
using ChinookApi.Models;

namespace ChinookApi.Features.Invoices;

public record GetAllInvoicesQuery(int Page, int PageSize) : IRequest<PagedResult<Invoice>>;

public class GetAllInvoicesHandler(ChinookContext db) : IRequestHandler<GetAllInvoicesQuery, PagedResult<Invoice>>
{
    public async Task<PagedResult<Invoice>> Handle(GetAllInvoicesQuery request, CancellationToken cancellationToken)
    {
        var query = db.Invoices.Include(i => i.Customer);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(i => i.InvoiceDate).Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);
        return new PagedResult<Invoice>(total, request.Page, request.PageSize, items);
    }
}
