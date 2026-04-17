using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChinookApi.Data;
using ChinookApi.Models;

namespace ChinookApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArtistsController : ControllerBase
{
    private readonly ChinookContext _db;
    public ArtistsController(ChinookContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = _db.Artists.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(a => a.Name != null && EF.Functions.Like(a.Name, $"%{search}%"));
        var total = await query.CountAsync();
        var items = await query.OrderBy(a => a.Name).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { total, page, pageSize, items });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var artist = await _db.Artists.Include(a => a.Albums).FirstOrDefaultAsync(a => a.ArtistId == id);
        if (artist == null) return NotFound();
        return Ok(artist);
    }
}
