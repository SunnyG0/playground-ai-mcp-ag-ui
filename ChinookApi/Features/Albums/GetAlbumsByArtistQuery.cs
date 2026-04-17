using MediatR;
using Microsoft.EntityFrameworkCore;
using ChinookApi.Data;
using ChinookApi.Models;

namespace ChinookApi.Features.Albums;

public record GetAlbumsByArtistQuery(int ArtistId) : IRequest<List<Album>>;

public class GetAlbumsByArtistHandler(ChinookContext db) : IRequestHandler<GetAlbumsByArtistQuery, List<Album>>
{
    public Task<List<Album>> Handle(GetAlbumsByArtistQuery request, CancellationToken cancellationToken) =>
        db.Albums.Include(a => a.Artist).Where(a => a.ArtistId == request.ArtistId).OrderBy(a => a.Title).ToListAsync(cancellationToken);
}
