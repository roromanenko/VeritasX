import 'react-date-range/dist/styles.css';         // Main style file
import 'react-date-range/dist/theme/default.css';  // Theme CSS
import { Outlet, Route, Routes, useNavigate } from 'react-router-dom'
import Landing from './pages/Landing'
import { ProtectedRoute } from './auth/ProtectedRoute';
import Requests from './pages/Requests';
import AuthProvider, { useAuth } from './auth/AuthContext';
import Login from './pages/Login';
import { Link } from 'react-router-dom';

function App() {

  return (
    <AuthProvider>
      <Navigation />

      <div className="app">
        <Routes>
          <Route index element={<Landing />} />
          <Route path='requests'
            element={
              <ProtectedRoute>
                <Requests />
              </ProtectedRoute>
            } />
          <Route path='login' element={<Login />} />
        </Routes>
      </div>
    </AuthProvider>
  )
}

const Navigation = () => {
  const { token, onLogout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    onLogout();
    navigate("/login");
  };

  return (
    <nav className="navbar">
      <ul className="nav-links">
        <li><Link to="/">Landing</Link></li>
        <li><Link to="/requests">Requests</Link></li>
        {!token && <li><Link to="/login">Login</Link></li>}
        {token && <li><Link to='/' onClick={(e) => { e.preventDefault(); handleLogout() }}>Logout</Link></li>}
      </ul>
    </nav>
  )
};

export default App