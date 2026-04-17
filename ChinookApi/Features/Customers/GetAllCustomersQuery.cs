using MediatR;
using Microsoft.EntityFrameworkCore;
using ChinookApi.Data;
using ChinookApi.Features.Common;
using ChinookApi.Models;

namespace ChinookApi.Features.Customers;

public record GetAllCustomersQuery(string? Search, int Page, int PageSize) : IRequest<PagedResult<Customer>>;

public class GetAllCustomersHandler(ChinookContext db) : IRequestHandler<GetAllCustomersQuery, PagedResult<Customer>>
{
    public async Task<PagedResult<Customer>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        var query = db.Customers.AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(c => EF.Functions.Like(c.FirstName + " " + c.LastName, $"%{request.Search}%") || EF.Functions.Like(c.Email, $"%{request.Search}%"));
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(c => c.LastName).ThenBy(c => c.FirstName).Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);
        return new PagedResult<Customer>(total, request.Page, request.PageSize, items);
    }
}
