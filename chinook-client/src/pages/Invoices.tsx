import { useState, useEffect, useCallback } from 'react';
import { invoicesApi } from '../api';
import type { Invoice, PagedResult } from '../api';
import Pagination from '../components/Pagination';

export default function Invoices() {
  const [data, setData] = useState<PagedResult<Invoice> | null>(null);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const load = useCallback(async () => {
    setLoading(true); setError('');
    try { setData(await invoicesApi.getAll({ page, pageSize: 20 })); }
    catch { setError('Failed to load invoices. Is the API running?'); }
    finally { setLoading(false); }
  }, [page]);

  useEffect(() => { load(); }, [load]);

  return (
    <div>
      <h1 className="page-title">Invoices</h1>
      {loading && <div className="loading">Loading...</div>}
      {error && <div className="error">{error}</div>}
      {data && !loading && (
        <>
          <table>
            <thead><tr><th>#</th><th>Customer</th><th>Date</th><th>Country</th><th>Total</th></tr></thead>
            <tbody>{data.items.map(i => (
              <tr key={i.invoiceId}>
                <td>{i.invoiceId}</td>
                <td>{i.customer ? `${i.customer.firstName} ${i.customer.lastName}` : i.customerId}</td>
                <td>{new Date(i.invoiceDate).toLocaleDateString()}</td>
                <td>{i.customer?.country ?? '—'}</td>
                <td>${i.total.toFixed(2)}</td>
              </tr>
            ))}</tbody>
          </table>
          <Pagination page={page} pageSize={20} total={data.total} onPage={setPage} />
        </>
      )}
    </div>
  );
}
