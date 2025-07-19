import 'react-date-range/dist/styles.css';         // Main style file
import 'react-date-range/dist/theme/default.css';  // Theme CSS
import { Route, Routes, useNavigate } from 'react-router-dom'
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
    <nav>
      <Link to="/">Landing</Link>
      <Link to="/requests">Requests</Link>
      <Link to="/login">Login</Link>
      {token && <Link onClick={(e) => { e.preventDefault(); handleLogout() }}>Logout</Link>}
    </nav>
  )
};

export default App