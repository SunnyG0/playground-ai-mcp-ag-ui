using MediatR;
using Microsoft.EntityFrameworkCore;
using ChinookApi.Data;
using ChinookApi.Models;

namespace ChinookApi.Features.Tracks;

public record GetTrackByIdQuery(int Id) : IRequest<Track?>;

public class GetTrackByIdHandler(ChinookContext db) : IRequestHandler<GetTrackByIdQuery, Track?>
{
    public Task<Track?> Handle(GetTrackByIdQuery request, CancellationToken cancellationToken) =>
        db.Tracks.Include(t => t.Album).ThenInclude(a => a!.Artist).Include(t => t.Genre).Include(t => t.MediaType).FirstOrDefaultAsync(t => t.TrackId == request.Id, cancellationToken);
}
