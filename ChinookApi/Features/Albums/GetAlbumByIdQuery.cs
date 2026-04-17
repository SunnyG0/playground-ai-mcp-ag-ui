using MediatR;
using Microsoft.EntityFrameworkCore;
using ChinookApi.Data;
using ChinookApi.Models;

namespace ChinookApi.Features.Albums;

public record GetAlbumByIdQuery(int Id) : IRequest<Album?>;

public class GetAlbumByIdHandler(ChinookContext db) : IRequestHandler<GetAlbumByIdQuery, Album?>
{
    public Task<Album?> Handle(GetAlbumByIdQuery request, CancellationToken cancellationToken) =>
        db.Albums.Include(a => a.Artist).Include(a => a.Tracks).FirstOrDefaultAsync(a => a.AlbumId == request.Id, cancellationToken);
}
