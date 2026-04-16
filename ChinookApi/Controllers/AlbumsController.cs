using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChinookApi.Data;

namespace ChinookApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlbumsController : ControllerBase
{
    private readonly ChinookContext _db;
    public AlbumsController(ChinookContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = _db.Albums.Include(a => a.Artist).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(a => a.Title.Contains(search));
        var total = await query.CountAsync();
        var items = await query.OrderBy(a => a.Title).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { total, page, pageSize, items });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var album = await _db.Albums.Include(a => a.Artist).Include(a => a.Tracks).FirstOrDefaultAsync(a => a.AlbumId == id);
        if (album == null) return NotFound();
        return Ok(album);
    }

    [HttpGet("artist/{artistId}")]
    public async Task<IActionResult> GetByArtist(int artistId)
    {
        var albums = await _db.Albums.Include(a => a.Artist).Where(a => a.ArtistId == artistId).OrderBy(a => a.Title).ToListAsync();
        return Ok(albums);
    }
}
