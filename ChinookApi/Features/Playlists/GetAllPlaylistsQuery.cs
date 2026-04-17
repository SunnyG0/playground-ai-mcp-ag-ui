using MediatR;
using Microsoft.EntityFrameworkCore;
using ChinookApi.Data;
using ChinookApi.Models;

namespace ChinookApi.Features.Playlists;

public record GetAllPlaylistsQuery : IRequest<List<Playlist>>;

public class GetAllPlaylistsHandler(ChinookContext db) : IRequestHandler<GetAllPlaylistsQuery, List<Playlist>>
{
    public Task<List<Playlist>> Handle(GetAllPlaylistsQuery request, CancellationToken cancellationToken) =>
        db.Playlists.OrderBy(p => p.Name).ToListAsync(cancellationToken);
}
