import { useState, useEffect } from 'react';
import { playlistsApi } from '../api';
import type { Playlist } from '../api';

export default function Playlists() {
  const [playlists, setPlaylists] = useState<Playlist[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    setLoading(true);
    playlistsApi.getAll().then(setPlaylists).catch(() => setError('Failed to load playlists.')).finally(() => setLoading(false));
  }, []);

  return (
    <div>
      <h1 className="page-title">Playlists</h1>
      {loading && <div className="loading">Loading...</div>}
      {error && <div className="error">{error}</div>}
      {!loading && !error && (
        <table>
          <thead><tr><th>#</th><th>Name</th></tr></thead>
          <tbody>{playlists.map(p => (
            <tr key={p.playlistId}><td>{p.playlistId}</td><td>{p.name}</td></tr>
          ))}</tbody>
        </table>
      )}
    </div>
  );
}
