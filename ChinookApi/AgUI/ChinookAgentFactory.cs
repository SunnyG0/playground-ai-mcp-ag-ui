using System.ComponentModel;
using MediatR;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using ChinookApi.Features.Albums;
using ChinookApi.Features.Artists;
using ChinookApi.Features.Tracks;
using ChinookApi.Features.Genres;
using ChinookApi.Features.Playlists;
using ChinookApi.Features.Customers;

namespace ChinookApi.AgUI;

/// <summary>
/// Builds a Microsoft Agent Framework <see cref="AIAgent"/> wired up with Chinook database
/// tools so it can answer natural-language questions about the music catalogue.
/// </summary>
public sealed class ChinookAgentFactory(IMediator mediator, IConfiguration configuration, ILoggerFactory loggerFactory)
{
    private const string Instructions =
        "You are a knowledgeable music catalogue assistant for the Chinook music store. " +
        "You help users discover artists, albums, tracks, genres, playlists, and customer information. " +
        "Use the available tools to look up information and provide helpful, accurate answers. " +
        "When presenting lists, use a clear and readable format.";

    /// <summary>Creates a new <see cref="AIAgent"/> instance for a single run.</summary>
    public AIAgent Create()
    {
        var apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException(
                "OpenAI API key is not configured. Set the 'OpenAI:ApiKey' configuration value " +
                "or the 'OpenAI__ApiKey' environment variable.");

        var model = configuration["OpenAI:Model"] ?? "gpt-4o-mini";

        AITool[] tools =
        [
            AIFunctionFactory.Create(SearchMusicAsync, new AIFunctionFactoryOptions { Name = "search_music", Description = "Search across artists, albums, and tracks in a single query. Returns the top matches from each category." }),
            AIFunctionFactory.Create(ExploreArtistAsync, new AIFunctionFactoryOptions { Name = "explore_artist", Description = "Look up an artist and browse their complete discography — every album they have released with a full track listing for each." }),
            AIFunctionFactory.Create(GetAlbumDetailsAsync, new AIFunctionFactoryOptions { Name = "get_album_details", Description = "Get detailed information about a specific album, including all its tracks." }),
            AIFunctionFactory.Create(GetPlaylistsAsync, new AIFunctionFactoryOptions { Name = "get_playlists", Description = "Browse all available playlists." }),
            AIFunctionFactory.Create(GetGenresAsync, new AIFunctionFactoryOptions { Name = "get_genres", Description = "List all available music genres." }),
            AIFunctionFactory.Create(GetTracksAsync, new AIFunctionFactoryOptions { Name = "get_tracks", Description = "Search for tracks, optionally filtering by genre ID (use get_genres to find genre IDs)." }),
            AIFunctionFactory.Create(GetCustomerInfoAsync, new AIFunctionFactoryOptions { Name = "get_customer_info", Description = "Look up customer information by name or email." }),
        ];

        return new OpenAIClient(apiKey)
            .GetChatClient(model)
            .AsAIAgent(instructions: Instructions, tools: [.. tools], loggerFactory: loggerFactory);
    }

    // ── Tool implementations ────────────────────────────────────────────────

    [Description("Search across artists, albums, and tracks in a single query.")]
    private async Task<string> SearchMusicAsync(
        [Description("The search term to look for")] string query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return "Please provide a search term.";

        var artistsTask = mediator.Send(new GetAllArtistsQuery(query, 1, 5), cancellationToken);
        var albumsTask = mediator.Send(new GetAllAlbumsQuery(query, 1, 5), cancellationToken);
        var tracksTask = mediator.Send(new GetAllTracksQuery(query, null, 1, 10), cancellationToken);

        await Task.WhenAll(artistsTask, albumsTask, tracksTask);

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Search results for \"{query}\":");

        var artists = artistsTask.Result;
        sb.AppendLine($"\nArtists ({artists.Total} total, showing {artists.Items.Count}):");
        if (artists.Items.Count == 0) sb.AppendLine("  (none found)");
        else foreach (var a in artists.Items) sb.AppendLine($"  • [{a.ArtistId}] {a.Name}");

        var albums = albumsTask.Result;
        sb.AppendLine($"\nAlbums ({albums.Total} total, showing {albums.Items.Count}):");
        if (albums.Items.Count == 0) sb.AppendLine("  (none found)");
        else foreach (var al in albums.Items) sb.AppendLine($"  • [{al.AlbumId}] {al.Title}  —  {al.Artist?.Name}");

        var tracks = tracksTask.Result;
        sb.AppendLine($"\nTracks ({tracks.Total} total, showing {tracks.Items.Count}):");
        if (tracks.Items.Count == 0) sb.AppendLine("  (none found)");
        else foreach (var t in tracks.Items)
        {
            var dur = TimeSpan.FromMilliseconds(t.Milliseconds);
            sb.AppendLine($"  • [{t.TrackId}] {t.Name}  —  {t.Album?.Title}  ({dur:m\\:ss})  [{t.Genre?.Name}]");
        }

        return sb.ToString();
    }

    [Description("Look up an artist and browse their complete discography.")]
    private async Task<string> ExploreArtistAsync(
        [Description("The artist's name (or part of it) to search for")] string artistName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(artistName))
            return "Please provide an artist name.";

        var artists = await mediator.Send(new GetAllArtistsQuery(artistName, 1, 5), cancellationToken);
        if (artists.Items.Count == 0)
            return $"No artist found matching \"{artistName}\".";

        var sb = new System.Text.StringBuilder();

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
                sb.AppendLine($"  Tracks: {tracks.Count}  |  Duration: {(int)totalDuration.TotalMinutes}:{totalDuration.Seconds:D2}");
                foreach (var track in tracks)
                {
                    var dur = TimeSpan.FromMilliseconds(track.Milliseconds);
                    sb.AppendLine($"    [{track.TrackId}] {track.Name}  ({dur:m\\:ss})");
                }
            }
        }

        return sb.ToString();
    }

    [Description("Get detailed information about a specific album including all its tracks.")]
    private async Task<string> GetAlbumDetailsAsync(
        [Description("The album ID")] int albumId,
        CancellationToken cancellationToken = default)
    {
        var album = await mediator.Send(new GetAlbumByIdQuery(albumId), cancellationToken);
        if (album == null) return $"No album found with ID {albumId}.";

        var tracks = await mediator.Send(new GetTracksByAlbumQuery(albumId), cancellationToken);
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Album: {album.Title} (ID: {album.AlbumId})");
        sb.AppendLine($"Artist: {album.Artist?.Name}");
        sb.AppendLine($"Tracks: {tracks.Count}");

        foreach (var track in tracks)
        {
            var dur = TimeSpan.FromMilliseconds(track.Milliseconds);
            sb.AppendLine($"  [{track.TrackId}] {track.Name}  —  {track.Genre?.Name}  ({dur:m\\:ss})");
        }

        return sb.ToString();
    }

    [Description("Browse all available playlists.")]
    private async Task<string> GetPlaylistsAsync(CancellationToken cancellationToken = default)
    {
        var playlists = await mediator.Send(new GetAllPlaylistsQuery(), cancellationToken);
        if (playlists.Count == 0) return "No playlists found.";

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Playlists ({playlists.Count} total):");
        foreach (var pl in playlists)
            sb.AppendLine($"  • [{pl.PlaylistId}] {pl.Name}");
        return sb.ToString();
    }

    [Description("List all available music genres.")]
    private async Task<string> GetGenresAsync(CancellationToken cancellationToken = default)
    {
        var genres = await mediator.Send(new GetAllGenresQuery(), cancellationToken);
        if (genres.Count == 0) return "No genres found.";

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Genres ({genres.Count} total):");
        foreach (var g in genres)
            sb.AppendLine($"  • [{g.GenreId}] {g.Name}");
        return sb.ToString();
    }

    [Description("Search for tracks, optionally filtering by genre ID.")]
    private async Task<string> GetTracksAsync(
        [Description("Optional search term")] string? search,
        [Description("Optional genre ID to filter by (use get_genres to find IDs)")] int? genreId,
        [Description("Maximum number of results (default 20)")] int? limit,
        CancellationToken cancellationToken = default)
    {
        var tracks = await mediator.Send(new GetAllTracksQuery(search, genreId, 1, limit ?? 20), cancellationToken);
        if (tracks.Items.Count == 0) return "No tracks found.";

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Tracks ({tracks.Total} total, showing {tracks.Items.Count}):");
        foreach (var t in tracks.Items)
        {
            var dur = TimeSpan.FromMilliseconds(t.Milliseconds);
            sb.AppendLine($"  • [{t.TrackId}] {t.Name}  —  {t.Album?.Title}  [{t.Genre?.Name}]  ({dur:m\\:ss})");
        }
        return sb.ToString();
    }

    [Description("Look up customer information by name or email.")]
    private async Task<string> GetCustomerInfoAsync(
        [Description("Customer name or part of their name/email to search for")] string search,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(search))
            return "Please provide a search term.";

        var customers = await mediator.Send(new GetAllCustomersQuery(search, 1, 10), cancellationToken);
        if (customers.Items.Count == 0)
            return $"No customers found matching \"{search}\".";

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Customers ({customers.Total} total, showing {customers.Items.Count}):");
        foreach (var c in customers.Items)
            sb.AppendLine($"  • [{c.CustomerId}] {c.FirstName} {c.LastName}  <{c.Email}>  {c.Country}");
        return sb.ToString();
    }
}
