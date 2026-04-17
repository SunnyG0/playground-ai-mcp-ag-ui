using MediatR;
using Microsoft.EntityFrameworkCore;
using ChinookApi.Data;
using ChinookApi.Models;

namespace ChinookApi.Features.Artists;

public record GetArtistByIdQuery(int Id) : IRequest<Artist?>;

public class GetArtistByIdHandler(ChinookContext db) : IRequestHandler<GetArtistByIdQuery, Artist?>
{
    public Task<Artist?> Handle(GetArtistByIdQuery request, CancellationToken cancellationToken) =>
        db.Artists.Include(a => a.Albums).FirstOrDefaultAsync(a => a.ArtistId == request.Id, cancellationToken);
}
