import { useState, useEffect, useCallback } from 'react';
import { customersApi } from '../api';
import type { Customer, PagedResult } from '../api';
import Pagination from '../components/Pagination';

export default function Customers() {
  const [data, setData] = useState<PagedResult<Customer> | null>(null);
  const [search, setSearch] = useState('');
  const [input, setInput] = useState('');
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const load = useCallback(async () => {
    setLoading(true); setError('');
    try { setData(await customersApi.getAll({ search, page, pageSize: 20 })); }
    catch { setError('Failed to load customers. Is the API running?'); }
    finally { setLoading(false); }
  }, [search, page]);

  useEffect(() => { load(); }, [load]);

  const handleSearch = () => { setSearch(input); setPage(1); };

  return (
    <div>
      <h1 className="page-title">Customers</h1>
      <div className="search-bar">
        <input value={input} onChange={e => setInput(e.target.value)} onKeyDown={e => e.key === 'Enter' && handleSearch()} placeholder="Search by name or email..." />
        <button onClick={handleSearch}>Search</button>
      </div>
      {loading && <div className="loading">Loading...</div>}
      {error && <div className="error">{error}</div>}
      {data && !loading && (
        <>
          <table>
            <thead><tr><th>#</th><th>Name</th><th>Email</th><th>City</th><th>Country</th></tr></thead>
            <tbody>{data.items.map(c => (
              <tr key={c.customerId}>
                <td>{c.customerId}</td>
                <td>{c.firstName} {c.lastName}</td>
                <td>{c.email}</td>
                <td>{c.city ?? '—'}</td>
                <td>{c.country ?? '—'}</td>
              </tr>
            ))}</tbody>
          </table>
          <Pagination page={page} pageSize={20} total={data.total} onPage={setPage} />
        </>
      )}
    </div>
  );
}
