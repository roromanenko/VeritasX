import { Navigate, useLocation } from "react-router-dom";
import { JSX } from "react";
import { useAuth } from "./AuthContext";

type ProtectedRouteProps = {
    children: React.ReactNode;
};

export const ProtectedRoute = ({ children }: ProtectedRouteProps) => {
    const location = useLocation();
    const { token } = useAuth();

    return token
    ? children
    : <Navigate to="/landing" replace state={{from: location}} />;
}