# Chinook Full-Stack App

A full-stack application using the [Chinook](https://github.com/lerocha/chinook-database) music store database.

## Projects

- **ChinookApi/** — ASP.NET Core Web API with EF Core + SQLite
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
