using System.ComponentModel;
using System.Text;
using MediatR;
using ModelContextProtocol.Server;
using ChinookApi.Features.Albums;
using ChinookApi.Features.Artists;
using ChinookApi.Features.Tracks;

namespace ChinookApi.Mcp;

[McpServerToolType]
public sealed class ArtistCatalogTool(IMediator mediator)
{
    [Description("Look up an artist and browse their complete discography — every album they have released with a full track listing for each. Great for discovering an artist's body of work.")]
    [McpServerTool(Name = "explore_artist")]
    public async Task<string> ExploreArtistAsync(
        [Description("The artist's name (or part of it) to search for")] string artistName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(artistName))
            return "Please provide an artist name.";

        var artists = await mediator.Send(new GetAllArtistsQuery(artistName, 1, 5), cancellationToken);

        if (artists.Items.Count == 0)
            return $"No artist found matching \"{artistName}\".";

        var sb = new StringBuilder();

        if (artists.Items.Count > 1)
        {
            sb.AppendLine($"Multiple artists matched \"{artistName}\". Showing all:");
            foreach (var match in artists.Items)
                sb.AppendLine($"  • [{match.ArtistId}] {match.Name}");
            sb.AppendLine();
        }

        foreach (var artist in artists.Items)
        {
            var albums = await mediator.Send(new GetAlbumsByArtistQuery(artist.ArtistId), cancellationToken);

            sb.AppendLine($"Artist: {artist.Name} (ID: {artist.ArtistId})");
            sb.AppendLine($"Albums: {albums.Count}");

            foreach (var album in albums)
            {
                var tracks = await mediator.Send(new GetTracksByAlbumQuery(album.AlbumId), cancellationToken);
                var totalMs = tracks.Sum(t => t.Milliseconds);
                var totalDuration = TimeSpan.FromMilliseconds(totalMs);

                sb.AppendLine($"\n  Album: {album.Title} (ID: {album.AlbumId})");
                sb.AppendLine($"  Tracks: {tracks.Count}  |  Total duration: {(int)totalDuration.TotalMinutes}:{totalDuration.Seconds:D2}");
                foreach (var track in tracks)
                {
                    var duration = TimeSpan.FromMilliseconds(track.Milliseconds);
                    sb.AppendLine($"    [{track.TrackId}] {track.Name}  ({duration:m\\:ss})");
                }
            }

            if (artists.Items.Count > 1)
                sb.AppendLine(new string('-', 40));
        }

        return sb.ToString();
    }
}
