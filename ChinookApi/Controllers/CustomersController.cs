using MediatR;
using Microsoft.AspNetCore.Mvc;
using ChinookApi.Features.Customers;

namespace ChinookApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        Ok(await mediator.Send(new GetAllCustomersQuery(search, page, pageSize)));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var customer = await mediator.Send(new GetCustomerByIdQuery(id));
        if (customer == null) return NotFound();
        return Ok(customer);
    }
}
