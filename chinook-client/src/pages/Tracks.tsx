import { useState, useEffect, useCallback } from 'react';
import { tracksApi, genresApi } from '../api';
import type { Track, Genre, PagedResult } from '../api';
import Pagination from '../components/Pagination';

function formatDuration(ms: number) {
  const s = Math.floor(ms / 1000);
  return `${Math.floor(s / 60)}:${String(s % 60).padStart(2, '0')}`;
}

export default function Tracks() {
  const [data, setData] = useState<PagedResult<Track> | null>(null);
  const [genres, setGenres] = useState<Genre[]>([]);
  const [search, setSearch] = useState('');
  const [input, setInput] = useState('');
  const [genreId, setGenreId] = useState<number | undefined>();
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => { genresApi.getAll().then(setGenres).catch(() => {}); }, []);

  const load = useCallback(async () => {
    setLoading(true); setError('');
    try { setData(await tracksApi.getAll({ search, genreId, page, pageSize: 20 })); }
    catch { setError('Failed to load tracks. Is the API running?'); }
    finally { setLoading(false); }
  }, [search, genreId, page]);

  useEffect(() => { load(); }, [load]);

  const handleSearch = () => { setSearch(input); setPage(1); };

  return (
    <div>
      <h1 className="page-title">Tracks</h1>
      <div className="search-bar">
        <input value={input} onChange={e => setInput(e.target.value)} onKeyDown={e => e.key === 'Enter' && handleSearch()} placeholder="Search tracks..." />
        <select value={genreId ?? ''} onChange={e => { setGenreId(e.target.value ? Number(e.target.value) : undefined); setPage(1); }} style={{ padding: '0.6rem', border: '1px solid #ddd', borderRadius: 4, fontSize: '1rem' }}>
          <option value="">All Genres</option>
          {genres.map(g => <option key={g.genreId} value={g.genreId}>{g.name}</option>)}
        </select>
        <button onClick={handleSearch}>Search</button>
      </div>
      {loading && <div className="loading">Loading...</div>}
      {error && <div className="error">{error}</div>}
      {data && !loading && (
        <>
          <table>
            <thead><tr><th>#</th><th>Name</th><th>Album</th><th>Genre</th><th>Duration</th><th>Price</th></tr></thead>
            <tbody>{data.items.map(t => (
              <tr key={t.trackId}>
                <td>{t.trackId}</td>
                <td>{t.name}</td>
                <td>{t.album?.title ?? '—'}</td>
                <td>{t.genre?.name ? <span className="badge">{t.genre.name}</span> : '—'}</td>
                <td>{formatDuration(t.milliseconds)}</td>
                <td>${t.unitPrice.toFixed(2)}</td>
              </tr>
            ))}</tbody>
          </table>
          <Pagination page={page} pageSize={20} total={data.total} onPage={setPage} />
        </>
      )}
    </div>
  );
}
