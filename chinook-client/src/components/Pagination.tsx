interface Props {
  page: number;
  pageSize: number;
  total: number;
  onPage: (p: number) => void;
}
export default function Pagination({ page, pageSize, total, onPage }: Props) {
  const totalPages = Math.ceil(total / pageSize);
  if (totalPages <= 1) return null;
  return (
    <div className="pagination">
      <button disabled={page <= 1} onClick={() => onPage(1)}>«</button>
      <button disabled={page <= 1} onClick={() => onPage(page - 1)}>‹</button>
      <span>Page {page} of {totalPages} ({total} total)</span>
      <button disabled={page >= totalPages} onClick={() => onPage(page + 1)}>›</button>
      <button disabled={page >= totalPages} onClick={() => onPage(totalPages)}>»</button>
    </div>
  );
}
