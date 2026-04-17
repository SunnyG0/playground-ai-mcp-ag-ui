using MediatR;
using Microsoft.AspNetCore.Mvc;
using ChinookApi.Features.Invoices;

namespace ChinookApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        Ok(await mediator.Send(new GetAllInvoicesQuery(page, pageSize)));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var invoice = await mediator.Send(new GetInvoiceByIdQuery(id));
        if (invoice == null) return NotFound();
        return Ok(invoice);
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetByCustomer(int customerId) =>
        Ok(await mediator.Send(new GetInvoicesByCustomerQuery(customerId)));
}
