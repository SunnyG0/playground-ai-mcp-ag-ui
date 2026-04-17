using MediatR;
using Microsoft.EntityFrameworkCore;
using ChinookApi.Data;
using ChinookApi.Models;

namespace ChinookApi.Features.Invoices;

public record GetCustomerInvoicesWithLinesQuery(int CustomerId) : IRequest<List<Invoice>>;

public class GetCustomerInvoicesWithLinesHandler(ChinookContext db) : IRequestHandler<GetCustomerInvoicesWithLinesQuery, List<Invoice>>
{
    public Task<List<Invoice>> Handle(GetCustomerInvoicesWithLinesQuery request, CancellationToken cancellationToken) =>
        db.Invoices
            .Where(i => i.CustomerId == request.CustomerId)
            .Include(i => i.Lines).ThenInclude(l => l.Track)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync(cancellationToken);
}
