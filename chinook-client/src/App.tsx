import { BrowserRouter, Routes, Route, NavLink } from 'react-router-dom';
import Artists from './pages/Artists';
import Albums from './pages/Albums';
import Tracks from './pages/Tracks';
import Playlists from './pages/Playlists';
import Customers from './pages/Customers';
import Invoices from './pages/Invoices';
import Agent from './pages/Agent';
import './App.css';

export default function App() {
  return (
    <BrowserRouter>
      <div className="app">
        <nav className="navbar">
          <span className="nav-brand">🎵 Chinook</span>
          <div className="nav-links">
            <NavLink to="/" end className={({ isActive }) => isActive ? 'active' : ''}>Artists</NavLink>
            <NavLink to="/albums" className={({ isActive }) => isActive ? 'active' : ''}>Albums</NavLink>
            <NavLink to="/tracks" className={({ isActive }) => isActive ? 'active' : ''}>Tracks</NavLink>
            <NavLink to="/playlists" className={({ isActive }) => isActive ? 'active' : ''}>Playlists</NavLink>
            <NavLink to="/customers" className={({ isActive }) => isActive ? 'active' : ''}>Customers</NavLink>
            <NavLink to="/invoices" className={({ isActive }) => isActive ? 'active' : ''}>Invoices</NavLink>
            <NavLink to="/agent" className={({ isActive }) => isActive ? 'active' : ''}>🤖 Agent</NavLink>
          </div>
        </nav>
        <main className="main-content">
          <Routes>
            <Route path="/" element={<Artists />} />
            <Route path="/albums" element={<Albums />} />
            <Route path="/tracks" element={<Tracks />} />
            <Route path="/playlists" element={<Playlists />} />
            <Route path="/customers" element={<Customers />} />
            <Route path="/invoices" element={<Invoices />} />
            <Route path="/agent" element={<Agent />} />
          </Routes>
        </main>
      </div>
    </BrowserRouter>
  );
}
