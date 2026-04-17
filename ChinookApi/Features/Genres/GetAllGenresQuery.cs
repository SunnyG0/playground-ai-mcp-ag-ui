using MediatR;
using Microsoft.EntityFrameworkCore;
using ChinookApi.Data;
using ChinookApi.Models;

namespace ChinookApi.Features.Genres;

public record GetAllGenresQuery : IRequest<List<Genre>>;

public class GetAllGenresHandler(ChinookContext db) : IRequestHandler<GetAllGenresQuery, List<Genre>>
{
    public Task<List<Genre>> Handle(GetAllGenresQuery request, CancellationToken cancellationToken) =>
        db.Genres.OrderBy(g => g.Name).ToListAsync(cancellationToken);
}
