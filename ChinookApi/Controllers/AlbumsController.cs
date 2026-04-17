using MediatR;
using Microsoft.AspNetCore.Mvc;
using ChinookApi.Features.Albums;

namespace ChinookApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlbumsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        Ok(await mediator.Send(new GetAllAlbumsQuery(search, page, pageSize)));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var album = await mediator.Send(new GetAlbumByIdQuery(id));
        if (album == null) return NotFound();
        return Ok(album);
    }

    [HttpGet("artist/{artistId}")]
    public async Task<IActionResult> GetByArtist(int artistId) =>
        Ok(await mediator.Send(new GetAlbumsByArtistQuery(artistId)));
}
