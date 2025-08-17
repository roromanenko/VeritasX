import { useState } from "react";
import { useAuth } from "../auth/AuthContext";
import { useLocation } from "react-router-dom";

export const Login = () => {
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [errorMessage, setErrorMessage] = useState("");
    const { onLogin } = useAuth();
    const location = useLocation();

    async function handleLogin(e: React.FormEvent) {
       try {
           e.preventDefault();
           const message = await onLogin(username, password, location.state?.from?.pathname);
           setErrorMessage(message);
       }
       catch {
           setErrorMessage("Something went wrong");
       }
    }

    return (
        <>
            <div className="login-page">
                {errorMessage && (
                    <div className="login-error-form">
                        <span>{errorMessage}</span>
                    </div>
                )}
                <div className="login-form">
                    <h2>Welcome Back</h2>
                    <form onSubmit={handleLogin}>
                        <input placeholder="Username" required value={username} onChange={(e) => setUsername(e.target.value)} />
                        <input type="password" placeholder="Password" required value={password} onChange={(e) => setPassword(e.target.value)} />
                        <button className="primary-button" type="submit">Sign In</button>
                    </form>
                </div>
            </div>
        </>
    );
}
