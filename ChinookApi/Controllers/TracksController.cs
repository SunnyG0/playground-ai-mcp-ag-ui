using MediatR;
using Microsoft.AspNetCore.Mvc;
using ChinookApi.Features.Tracks;

namespace ChinookApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TracksController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int? genreId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        Ok(await mediator.Send(new GetAllTracksQuery(search, genreId, page, pageSize)));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var track = await mediator.Send(new GetTrackByIdQuery(id));
        if (track == null) return NotFound();
        return Ok(track);
    }

    [HttpGet("album/{albumId}")]
    public async Task<IActionResult> GetByAlbum(int albumId) =>
        Ok(await mediator.Send(new GetTracksByAlbumQuery(albumId)));
}
