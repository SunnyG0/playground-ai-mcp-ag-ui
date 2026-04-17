using MediatR;
using Microsoft.EntityFrameworkCore;
using ChinookApi.Data;
using ChinookApi.Models;

namespace ChinookApi.Features.Invoices;

public record GetInvoiceByIdQuery(int Id) : IRequest<Invoice?>;

public class GetInvoiceByIdHandler(ChinookContext db) : IRequestHandler<GetInvoiceByIdQuery, Invoice?>
{
    public Task<Invoice?> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken) =>
        db.Invoices.Include(i => i.Customer).Include(i => i.Lines).ThenInclude(l => l.Track).FirstOrDefaultAsync(i => i.InvoiceId == request.Id, cancellationToken);
}
