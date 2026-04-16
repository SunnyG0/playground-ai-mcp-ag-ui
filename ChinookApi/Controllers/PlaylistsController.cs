using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChinookApi.Data;

namespace ChinookApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlaylistsController : ControllerBase
{
    private readonly ChinookContext _db;
    public PlaylistsController(ChinookContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Playlists.OrderBy(p => p.Name).ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var playlist = await _db.Playlists.Include(p => p.Tracks).ThenInclude(t => t.Album).FirstOrDefaultAsync(p => p.PlaylistId == id);
        if (playlist == null) return NotFound();
        return Ok(playlist);
    }
}
