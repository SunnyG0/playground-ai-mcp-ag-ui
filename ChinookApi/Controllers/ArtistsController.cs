using MediatR;
using Microsoft.AspNetCore.Mvc;
using ChinookApi.Features.Artists;

namespace ChinookApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArtistsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        Ok(await mediator.Send(new GetAllArtistsQuery(search, page, pageSize)));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var artist = await mediator.Send(new GetArtistByIdQuery(id));
        if (artist == null) return NotFound();
        return Ok(artist);
    }
}
