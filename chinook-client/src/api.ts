import axios from 'axios';

const BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5100';

const api = axios.create({ baseURL: BASE_URL });

export interface PagedResult<T> {
  total: number;
  page: number;
  pageSize: number;
  items: T[];
}

export interface Artist { artistId: number; name: string; }
export interface Album { albumId: number; title: string; artistId: number; artist?: Artist; }
export interface Genre { genreId: number; name: string; }
export interface Track { trackId: number; name: string; albumId?: number; album?: Album; genreId?: number; genre?: Genre; composer?: string; milliseconds: number; unitPrice: number; }
export interface Playlist { playlistId: number; name: string; }
export interface Customer { customerId: number; firstName: string; lastName: string; email: string; country?: string; city?: string; }
export interface InvoiceLine { invoiceLineId: number; trackId: number; track?: Track; unitPrice: number; quantity: number; }
export interface Invoice { invoiceId: number; customerId: number; customer?: Customer; invoiceDate: string; total: number; lines?: InvoiceLine[]; }

export const artistsApi = {
  getAll: (params?: { search?: string; page?: number; pageSize?: number }) =>
    api.get<PagedResult<Artist>>('/api/artists', { params }).then(r => r.data),
  getById: (id: number) => api.get<Artist>(`/api/artists/${id}`).then(r => r.data),
};

export const albumsApi = {
  getAll: (params?: { search?: string; page?: number; pageSize?: number }) =>
    api.get<PagedResult<Album>>('/api/albums', { params }).then(r => r.data),
  getById: (id: number) => api.get<Album>(`/api/albums/${id}`).then(r => r.data),
  getByArtist: (artistId: number) => api.get<Album[]>(`/api/albums/artist/${artistId}`).then(r => r.data),
};

export const tracksApi = {
  getAll: (params?: { search?: string; genreId?: number; page?: number; pageSize?: number }) =>
    api.get<PagedResult<Track>>('/api/tracks', { params }).then(r => r.data),
  getById: (id: number) => api.get<Track>(`/api/tracks/${id}`).then(r => r.data),
  getByAlbum: (albumId: number) => api.get<Track[]>(`/api/tracks/album/${albumId}`).then(r => r.data),
};

export const genresApi = {
  getAll: () => api.get<Genre[]>('/api/genres').then(r => r.data),
};

export const playlistsApi = {
  getAll: () => api.get<Playlist[]>('/api/playlists').then(r => r.data),
  getById: (id: number) => api.get<Playlist>(`/api/playlists/${id}`).then(r => r.data),
};

export const customersApi = {
  getAll: (params?: { search?: string; page?: number; pageSize?: number }) =>
    api.get<PagedResult<Customer>>('/api/customers', { params }).then(r => r.data),
  getById: (id: number) => api.get<Customer>(`/api/customers/${id}`).then(r => r.data),
};

export const invoicesApi = {
  getAll: (params?: { page?: number; pageSize?: number }) =>
    api.get<PagedResult<Invoice>>('/api/invoices', { params }).then(r => r.data),
  getById: (id: number) => api.get<Invoice>(`/api/invoices/${id}`).then(r => r.data),
};
