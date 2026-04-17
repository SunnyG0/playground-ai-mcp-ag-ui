using MediatR;
using Microsoft.AspNetCore.Mvc;
using ChinookApi.Features.Playlists;

namespace ChinookApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlaylistsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await mediator.Send(new GetAllPlaylistsQuery()));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var playlist = await mediator.Send(new GetPlaylistByIdQuery(id));
        if (playlist == null) return NotFound();
        return Ok(playlist);
    }
}
