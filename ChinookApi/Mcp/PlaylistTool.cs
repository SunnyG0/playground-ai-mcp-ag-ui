using System.ComponentModel;
using System.Text;
using MediatR;
using ModelContextProtocol.Server;
using ChinookApi.Features.Playlists;

namespace ChinookApi.Mcp;

[McpServerToolType]
public sealed class PlaylistTool(IMediator mediator)
{
    [Description("List all available playlists in the music store. Returns playlist names and IDs so you can then use browse_playlist to explore their contents.")]
    [McpServerTool(Name = "list_playlists")]
    public async Task<string> ListPlaylistsAsync(CancellationToken cancellationToken = default)
    {
        var playlists = await mediator.Send(new GetAllPlaylistsQuery(), cancellationToken);

        if (playlists.Count == 0)
            return "No playlists found.";

        var sb = new StringBuilder();
        sb.AppendLine($"Available playlists ({playlists.Count} total):");
        foreach (var p in playlists)
            sb.AppendLine($"  • [{p.PlaylistId}] {p.Name}");

        return sb.ToString();
    }

    [Description("View the full contents of a playlist by its ID — including every track, the album it comes from, and each track's duration. Use list_playlists first to find the playlist ID.")]
    [McpServerTool(Name = "browse_playlist")]
    public async Task<string> BrowsePlaylistAsync(
        [Description("The numeric ID of the playlist to browse")] int playlistId,
        CancellationToken cancellationToken = default)
    {
        var playlist = await mediator.Send(new GetPlaylistByIdQuery(playlistId), cancellationToken);

        if (playlist is null)
            return $"Playlist with ID {playlistId} not found.";

        var sb = new StringBuilder();
        sb.AppendLine($"Playlist: {playlist.Name} (ID: {playlist.PlaylistId})");
        sb.AppendLine($"Tracks: {playlist.Tracks.Count}");

        if (playlist.Tracks.Count == 0)
        {
            sb.AppendLine("  (empty playlist)");
            return sb.ToString();
        }

        var totalMs = playlist.Tracks.Sum(t => t.Milliseconds);
        var totalDuration = TimeSpan.FromMilliseconds(totalMs);
        sb.AppendLine($"Total duration: {(int)totalDuration.TotalMinutes}:{totalDuration.Seconds:D2}");
        sb.AppendLine();
        sb.AppendLine("Tracks:");

        int idx = 1;
        foreach (var t in playlist.Tracks)
        {
            var duration = TimeSpan.FromMilliseconds(t.Milliseconds);
            sb.AppendLine($"  {idx++,3}. [{t.TrackId}] {t.Name}  —  {t.Album?.Title}  ({duration:m\\:ss})");
        }

        return sb.ToString();
    }
}
