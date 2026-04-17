using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChinookApi.Data;

namespace ChinookApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TracksController : ControllerBase
{
    private readonly ChinookContext _db;
    public TracksController(ChinookContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int? genreId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = _db.Tracks.Include(t => t.Album).Include(t => t.Genre).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t => EF.Functions.Like(t.Name, $"%{search}%"));
        if (genreId.HasValue)
            query = query.Where(t => t.GenreId == genreId);
        var total = await query.CountAsync();
        var items = await query.OrderBy(t => t.Name).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { total, page, pageSize, items });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var track = await _db.Tracks.Include(t => t.Album).ThenInclude(a => a!.Artist).Include(t => t.Genre).Include(t => t.MediaType).FirstOrDefaultAsync(t => t.TrackId == id);
        if (track == null) return NotFound();
        return Ok(track);
    }

    [HttpGet("album/{albumId}")]
    public async Task<IActionResult> GetByAlbum(int albumId)
    {
        var tracks = await _db.Tracks.Include(t => t.Genre).Where(t => t.AlbumId == albumId).OrderBy(t => t.Name).ToListAsync();
        return Ok(tracks);
    }
}
