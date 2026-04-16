using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChinookApi.Data;

namespace ChinookApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly ChinookContext _db;
    public InvoicesController(ChinookContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = _db.Invoices.Include(i => i.Customer);
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(i => i.InvoiceDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { total, page, pageSize, items });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var invoice = await _db.Invoices.Include(i => i.Customer).Include(i => i.Lines).ThenInclude(l => l.Track).FirstOrDefaultAsync(i => i.InvoiceId == id);
        if (invoice == null) return NotFound();
        return Ok(invoice);
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetByCustomer(int customerId)
    {
        var invoices = await _db.Invoices.Where(i => i.CustomerId == customerId).OrderByDescending(i => i.InvoiceDate).ToListAsync();
        return Ok(invoices);
    }
}
