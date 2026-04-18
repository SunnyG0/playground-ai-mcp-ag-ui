# Chinook Full-Stack App

A full-stack application using the [Chinook](https://github.com/lerocha/chinook-database) music store database.

## Projects

- **ChinookApi/** — ASP.NET Core Web API with EF Core + SQLite, MCP server, and AG-UI server
- **chinook-client/** — Vite React TypeScript SPA

## Running the API

```bash
cd ChinookApi
dotnet run --urls http://localhost:5100
```

API will be available at `http://localhost:5100`.

## Running the Client

```bash
cd chinook-client
npm run dev
```

Client will be available at `http://localhost:5173`.

## API Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| GET | /api/artists | List artists (search, pagination) |
| GET | /api/artists/{id} | Artist with albums |
| GET | /api/albums | List albums (search, pagination) |
| GET | /api/albums/{id} | Album with tracks |
| GET | /api/albums/artist/{artistId} | Albums by artist |
| GET | /api/tracks | List tracks (search, genre filter, pagination) |
| GET | /api/tracks/{id} | Track detail |
| GET | /api/tracks/album/{albumId} | Tracks by album |
| GET | /api/genres | All genres |
| GET | /api/playlists | All playlists |
| GET | /api/playlists/{id} | Playlist with tracks |
| GET | /api/customers | List customers (search, pagination) |
| GET | /api/customers/{id} | Customer detail |
| GET | /api/invoices | List invoices (pagination) |
| GET | /api/invoices/{id} | Invoice with lines |
| GET | /api/invoices/customer/{customerId} | Invoices by customer |

## MCP Server

The API exposes an [MCP (Model Context Protocol)](https://modelcontextprotocol.io) server at `/mcp`.

Tools available:
- `search_music` — search across artists, albums, and tracks
- `explore_artist` — artist discography
- `get_album_details` — album with full track listing
- `get_playlist_details` — playlist contents
- `explore_genre` — tracks by genre
- `get_customer_history` — customer invoice history

## AG-UI Server

The API also exposes an [AG-UI](https://docs.ag-ui.com) endpoint at `POST /agui`, built with the [Microsoft Agent Framework](https://learn.microsoft.com/en-us/agent-framework/) (`Microsoft.Agents.AI`).

This endpoint is compatible with [CopilotKit](https://copilotkit.ai) and any other AG-UI–capable frontend.

### Configuration

Set your OpenAI API key before starting the server. Use either:

```bash
# Option A – environment variable (recommended for production)
export OpenAI__ApiKey="sk-..."
export OpenAI__Model="gpt-4o-mini"   # optional, default: gpt-4o-mini

# Option B – appsettings.Development.json (local development only, do not commit keys)
```

### Request format

`POST /agui` — `Content-Type: application/json`

```json
{
  "threadId": "thread-abc",
  "runId":    "run-xyz",
  "messages": [
    { "id": "msg-1", "role": "user", "content": "Who are the top rock artists in the catalogue?" }
  ]
}
```

### Response (SSE stream)

The endpoint returns `text/event-stream`. Events follow the AG-UI protocol:

```
data: {"type":"RUN_STARTED","threadId":"thread-abc","runId":"run-xyz","timestamp":...}

data: {"type":"STEP_STARTED","stepName":"agent","timestamp":...}

data: {"type":"TEXT_MESSAGE_START","messageId":"<uuid>","role":"assistant","timestamp":...}

data: {"type":"TEXT_MESSAGE_CONTENT","messageId":"<uuid>","delta":"Here are ","timestamp":...}

data: {"type":"TEXT_MESSAGE_CONTENT","messageId":"<uuid>","delta":"the top rock...","timestamp":...}

data: {"type":"TEXT_MESSAGE_END","messageId":"<uuid>","timestamp":...}

data: {"type":"STEP_FINISHED","stepName":"agent","timestamp":...}

data: {"type":"RUN_FINISHED","threadId":"thread-abc","runId":"run-xyz","outcome":"success","timestamp":...}
```

### Agent tools

The agent has access to the following Chinook database tools:

| Tool | Description |
|------|-------------|
| `search_music` | Search artists, albums, and tracks at once |
| `explore_artist` | Full discography for an artist |
| `get_album_details` | Album details with all tracks |
| `get_playlists` | List all playlists |
| `get_genres` | List all genres |
| `get_tracks` | Search/filter tracks |
| `get_customer_info` | Look up customers by name or email |
