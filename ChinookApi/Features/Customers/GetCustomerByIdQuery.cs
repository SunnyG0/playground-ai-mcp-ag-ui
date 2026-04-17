using MediatR;
using Microsoft.EntityFrameworkCore;
using ChinookApi.Data;
using ChinookApi.Models;

namespace ChinookApi.Features.Customers;

public record GetCustomerByIdQuery(int Id) : IRequest<Customer?>;

public class GetCustomerByIdHandler(ChinookContext db) : IRequestHandler<GetCustomerByIdQuery, Customer?>
{
    public Task<Customer?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken) =>
        db.Customers.FindAsync([request.Id], cancellationToken).AsTask();
}
