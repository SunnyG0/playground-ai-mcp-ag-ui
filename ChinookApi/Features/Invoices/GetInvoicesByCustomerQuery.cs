using MediatR;
using Microsoft.EntityFrameworkCore;
using ChinookApi.Data;
using ChinookApi.Models;

namespace ChinookApi.Features.Invoices;

public record GetInvoicesByCustomerQuery(int CustomerId) : IRequest<List<Invoice>>;

public class GetInvoicesByCustomerHandler(ChinookContext db) : IRequestHandler<GetInvoicesByCustomerQuery, List<Invoice>>
{
    public Task<List<Invoice>> Handle(GetInvoicesByCustomerQuery request, CancellationToken cancellationToken) =>
        db.Invoices.Where(i => i.CustomerId == request.CustomerId).OrderByDescending(i => i.InvoiceDate).ToListAsync(cancellationToken);
}
