import { useState } from "react";
import { useAuth } from "../auth/AuthContext";

function Login() {
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const { onLogin } = useAuth();

    async function handleLogin () {
        await onLogin(username, password);
    }

    return (
        <form onSubmit={(e) => e.preventDefault()}>
            <input value={username} onChange={(e) => setUsername(e.target.value)} placeholder="Username" />
            <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} placeholder="Password" />
            <button onClick={handleLogin}>Login</button>
        </form>
    );
}

export default Login;
