using MediatR;
using Microsoft.AspNetCore.Mvc;
using ChinookApi.Features.Genres;

namespace ChinookApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GenresController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await mediator.Send(new GetAllGenresQuery()));
}
