using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChinookApi.Data;

namespace ChinookApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GenresController : ControllerBase
{
    private readonly ChinookContext _db;
    public GenresController(ChinookContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Genres.OrderBy(g => g.Name).ToListAsync());
}
