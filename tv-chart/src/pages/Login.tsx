import { useState } from "react";
import { useAuth } from "../auth/AuthContext";
import { useLocation } from "react-router-dom";

function Login() {
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const { onLogin } = useAuth();
    const location = useLocation();

    async function handleLogin(e: React.FormEvent) {
        e.preventDefault();
        await onLogin(username, password, location.state?.from?.pathname);
    }

    return (
        <>
            <div className="login-page">
                <div className="login-form">
                    <h2>Welcome Back</h2>
                    <form onSubmit={handleLogin}>
                        <input placeholder="Username" required value={username} onChange={(e) => setUsername(e.target.value)} />
                        <input type="password" placeholder="Password" required value={password} onChange={(e) => setPassword(e.target.value)} />
                        <button type="submit">Sign In</button>
                    </form>
                </div>
            </div>
        </>
    );
}


{/* <form onSubmit={(e) => e.preventDefault()}>
    <input value={username} onChange={(e) => setUsername(e.target.value)} placeholder="Username" />
    <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} placeholder="Password" />
    <button onClick={handleLogin}>Login</button>
</form> */}
export default Login;
