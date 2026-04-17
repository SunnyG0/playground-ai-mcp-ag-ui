using MediatR;
using Microsoft.EntityFrameworkCore;
using ChinookApi.Data;
using ChinookApi.Models;

namespace ChinookApi.Features.Playlists;

public record GetPlaylistByIdQuery(int Id) : IRequest<Playlist?>;

public class GetPlaylistByIdHandler(ChinookContext db) : IRequestHandler<GetPlaylistByIdQuery, Playlist?>
{
    public Task<Playlist?> Handle(GetPlaylistByIdQuery request, CancellationToken cancellationToken) =>
        db.Playlists.Include(p => p.Tracks).ThenInclude(t => t.Album).FirstOrDefaultAsync(p => p.PlaylistId == request.Id, cancellationToken);
}
