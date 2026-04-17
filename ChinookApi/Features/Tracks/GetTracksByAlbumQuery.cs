using MediatR;
using Microsoft.EntityFrameworkCore;
using ChinookApi.Data;
using ChinookApi.Models;

namespace ChinookApi.Features.Tracks;

public record GetTracksByAlbumQuery(int AlbumId) : IRequest<List<Track>>;

public class GetTracksByAlbumHandler(ChinookContext db) : IRequestHandler<GetTracksByAlbumQuery, List<Track>>
{
    public Task<List<Track>> Handle(GetTracksByAlbumQuery request, CancellationToken cancellationToken) =>
        db.Tracks.Include(t => t.Genre).Where(t => t.AlbumId == request.AlbumId).OrderBy(t => t.Name).ToListAsync(cancellationToken);
}
