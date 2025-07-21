import 'react-date-range/dist/styles.css';         // Main style file
import 'react-date-range/dist/theme/default.css';  // Theme CSS
import { Route, Routes, useNavigate } from 'react-router-dom'
import { Landing } from './pages/Landing'
import { ProtectedRoute } from './auth/ProtectedRoute';
import { Requests } from './pages/Requests';
import { AuthProvider, useAuth } from './auth/AuthContext';
import { Login } from './pages/Login';
import { Link } from 'react-router-dom';
import { Chart } from './pages/Chart';

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
          <Route path='requests/:id'
            element={
              <ProtectedRoute>
                <Chart />
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
      <div className="nav-group">
        <Link to="/">Landing</Link>
      </div>

      <div className="nav-group">
        <Link to="/requests">Requests</Link>
      </div>

      <div className="nav-group">
        {!token && <Link to="/login">Login</Link>}
        {token && (
          <Link
            to="/"
            onClick={(e) => {
              e.preventDefault();
              handleLogout();
            }}
          >
            Logout
          </Link>
        )}
      </div>
    </nav>
  )
};

export default App