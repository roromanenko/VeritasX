import { createContext, useContext, useEffect, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { useApiProvider } from "../services/apiProvider";

const TOKEN_KEY = "auth-token"

type AuthProviderType = {
  token: string | null | undefined;
  onLogin: (username: string, password: string, redirectUrl: string | null) => void;
  onLogout: () => void;
}

const AuthContext = createContext<AuthProviderType>({
    token: '',
    onLogin: (username: string, password: string) => {},
    onLogout: () => {},
});

type AuthProviderProps = {
  children: React.ReactNode;
};

const AuthProvider = ({ children }: AuthProviderProps) => {
  const [token, setToken] = useState<string | null | undefined>();
  const navigate = useNavigate();
  const userApi = useApiProvider().getUserApi();

  useEffect(() => {
    let savedToken = sessionStorage.getItem(TOKEN_KEY);
    if (savedToken){
      setToken(savedToken);
    }
  })

  const handleLogin = async (username: string, password: string, redirectUrl: string | null) => {
    let response = await userApi.apiUserLoginPost({username, password})
    if (response.data.data?.token){
      sessionStorage.setItem(TOKEN_KEY, response.data.data?.token);
      setToken(response.data.data?.token);
      const origin = redirectUrl || '/';
      navigate(origin);
    }
  };

  const handleLogout = () => {
    setToken(null);
    sessionStorage.removeItem(TOKEN_KEY);
  };

  const value = {
    token,
    onLogin: handleLogin,
    onLogout: handleLogout,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = () => {
  return useContext(AuthContext);
};

export default AuthProvider;