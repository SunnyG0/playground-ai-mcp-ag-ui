import { useState, useEffect, useCallback } from 'react';
import { albumsApi } from '../api';
import type { Album, PagedResult } from '../api';
import Pagination from '../components/Pagination';

export default function Albums() {
  const [data, setData] = useState<PagedResult<Album> | null>(null);
  const [search, setSearch] = useState('');
  const [input, setInput] = useState('');
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const load = useCallback(async () => {
    setLoading(true); setError('');
    try { setData(await albumsApi.getAll({ search, page, pageSize: 20 })); }
    catch { setError('Failed to load albums. Is the API running?'); }
    finally { setLoading(false); }
  }, [search, page]);

  useEffect(() => { load(); }, [load]);

  const handleSearch = () => { setSearch(input); setPage(1); };

  return (
    <div>
      <h1 className="page-title">Albums</h1>
      <div className="search-bar">
        <input value={input} onChange={e => setInput(e.target.value)} onKeyDown={e => e.key === 'Enter' && handleSearch()} placeholder="Search albums..." />
        <button onClick={handleSearch}>Search</button>
      </div>
      {loading && <div className="loading">Loading...</div>}
      {error && <div className="error">{error}</div>}
      {data && !loading && (
        <>
          <table>
            <thead><tr><th>#</th><th>Title</th><th>Artist</th></tr></thead>
            <tbody>{data.items.map(a => (
              <tr key={a.albumId}><td>{a.albumId}</td><td>{a.title}</td><td>{a.artist?.name}</td></tr>
            ))}</tbody>
          </table>
          <Pagination page={page} pageSize={20} total={data.total} onPage={setPage} />
        </>
      )}
    </div>
  );
}
