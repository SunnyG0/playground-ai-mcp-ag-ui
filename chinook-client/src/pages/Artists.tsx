import { useState, useEffect, useCallback } from 'react';
import { artistsApi } from '../api';
import type { Artist, PagedResult } from '../api';
import Pagination from '../components/Pagination';

export default function Artists() {
  const [data, setData] = useState<PagedResult<Artist> | null>(null);
  const [search, setSearch] = useState('');
  const [input, setInput] = useState('');
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const load = useCallback(async () => {
    setLoading(true); setError('');
    try { setData(await artistsApi.getAll({ search, page, pageSize: 20 })); }
    catch { setError('Failed to load artists. Is the API running?'); }
    finally { setLoading(false); }
  }, [search, page]);

  useEffect(() => { load(); }, [load]);

  const handleSearch = () => { setSearch(input); setPage(1); };

  return (
    <div>
      <h1 className="page-title">Artists</h1>
      <div className="search-bar">
        <input value={input} onChange={e => setInput(e.target.value)} onKeyDown={e => e.key === 'Enter' && handleSearch()} placeholder="Search artists..." />
        <button onClick={handleSearch}>Search</button>
      </div>
      {loading && <div className="loading">Loading...</div>}
      {error && <div className="error">{error}</div>}
      {data && !loading && (
        <>
          <table>
            <thead><tr><th>#</th><th>Name</th></tr></thead>
            <tbody>{data.items.map(a => (
              <tr key={a.artistId}><td>{a.artistId}</td><td>{a.name}</td></tr>
            ))}</tbody>
          </table>
          <Pagination page={page} pageSize={20} total={data.total} onPage={setPage} />
        </>
      )}
    </div>
  );
}
