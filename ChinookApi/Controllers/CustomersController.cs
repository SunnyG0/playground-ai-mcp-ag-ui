using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChinookApi.Data;

namespace ChinookApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ChinookContext _db;
    public CustomersController(ChinookContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = _db.Customers.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => EF.Functions.Like(c.FirstName + " " + c.LastName, $"%{search}%") || EF.Functions.Like(c.Email, $"%{search}%"));
        var total = await query.CountAsync();
        var items = await query.OrderBy(c => c.LastName).ThenBy(c => c.FirstName).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { total, page, pageSize, items });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer == null) return NotFound();
        return Ok(customer);
    }
}
