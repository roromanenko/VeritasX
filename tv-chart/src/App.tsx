import 'react-date-range/dist/styles.css';
import 'react-date-range/dist/theme/default.css';
import { useState, useRef, useEffect } from 'react'
import { Route, Routes, useNavigate, NavLink, Link } from 'react-router-dom'
import { Bell, Zap, Settings as SettingsIcon } from 'lucide-react'
import { Dashboard } from './pages/Dashboard'
import { ProtectedRoute } from './auth/ProtectedRoute';
import { Requests } from './pages/Requests';
import { AuthProvider, useAuth } from './auth/AuthContext';
import { Login } from './pages/Login';
import { Landing } from './pages/Landing';
import { Chart } from './pages/Chart';
import { BotMonitor } from './pages/BotMonitor';
import { BotDetail } from './pages/BotDetail';
import { Settings } from './pages/Settings';

function App() {
  return (
    <AuthProvider>
      <Navigation />

      <div className="app">
        <Routes>
          <Route index element={
              <ProtectedRoute>
                <Dashboard />
              </ProtectedRoute>
            } />
          <Route path='requests'
            element={
              <ProtectedRoute>
                <Requests />
              </ProtectedRoute>
            } />
          <Route path='requests/:id'
            element={
              <ProtectedRoute>
                <Chart />
              </ProtectedRoute>
            } />
          <Route path='bots'
            element={
              <ProtectedRoute>
                <BotMonitor />
              </ProtectedRoute>
            } />
          <Route path='bots/:id'
            element={
              <ProtectedRoute>
                <BotDetail />
              </ProtectedRoute>
            } />
          <Route path='settings'
            element={
              <ProtectedRoute>
                <Settings />
              </ProtectedRoute>
            } />
          <Route path='login' element={<Login />} />
          <Route path='landing' element={<Landing />} />
        </Routes>
      </div>
    </AuthProvider>
  )
}

const navLinks = [
  { to: '/',         label: 'Dashboard',        end: true  },
  { to: '/requests', label: 'Backtests',  end: false },
  { to: '/bots',     label: 'Bots',              end: false },
]

type UserMenuProps = { onLogout: () => void }

function UserMenu({ onLogout }: UserMenuProps) {
  const [open, setOpen] = useState(false)
  const ref = useRef<HTMLDivElement>(null)
  const navigate = useNavigate()

  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (ref.current && !ref.current.contains(e.target as Node)) {
        setOpen(false)
      }
    }
    if (open) document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [open])

  return (
    <div ref={ref} style={{ position: 'relative' }}>
      <button className="nav-user-btn" onClick={() => setOpen(p => !p)}>
        <span className="nav-user-initials">VX</span>
      </button>
      {open && (
        <div className="nav-user-dropdown">
          <button
            className="nav-user-dropdown-item"
            onClick={() => { navigate('/settings'); setOpen(false) }}
          >
            <SettingsIcon size={14} /> Settings
          </button>
          <button
            className="nav-user-dropdown-item nav-user-dropdown-item--danger"
            onClick={() => { onLogout(); setOpen(false) }}
          >
            Logout
          </button>
        </div>
      )}
    </div>
  )
}

const Navigation = () => {
  const { token, onLogout } = useAuth()
  const navigate = useNavigate()

  const handleLogout = () => {
    onLogout()
    navigate('/login')
  }

  return (
    <header className="navbar">
      <Link to="/landing" className="nav-brand">
        <div className="nav-brand-icon">
          <Zap />
        </div>
        <div>
          <span className="nav-brand-name">VeritasX</span>
          <p className="nav-brand-subtitle">ALGORITHMIC TRADING PLATFORM</p>
        </div>
      </Link>

      <nav className="nav-links">
        {navLinks.map(link => (
          <NavLink
            key={link.to}
            to={link.to}
            end={link.end}
            className={({ isActive }) => `nav-link${isActive ? ' active' : ''}`}
          >
            {link.label}
          </NavLink>
        ))}
      </nav>

      <div className="nav-actions">
        <button className="nav-bell-btn" aria-label="Notifications">
          <Bell />
        </button>
        {!token
          ? <Link to="/login" className="nav-link">Login</Link>
          : <UserMenu onLogout={handleLogout} />
        }
      </div>
    </header>
  )
}

export default App
