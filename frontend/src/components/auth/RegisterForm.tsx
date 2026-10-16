import { useState } from "react";
import { useNavigate } from "react-router-dom";

import { register } from "@/api/auth";
import { TokenStorage } from "@/utils/tokenStorage";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Alert, AlertDescription } from "@/components/ui/alert";

export function RegisterForm() {
    const navigate = useNavigate();

    const [username, setUsername] = useState("");
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");

    const [loading, setLoading] = useState(false);
    const [error, setError] = useState("");

    async function handleSubmit(e: React.SubmitEvent) {
        e.preventDefault();

        setLoading(true);
        setError("");

        const result = await register({
            username,
            email,
            password,
        });

        if ("token" in result) {
            TokenStorage.set(result.token);
            navigate("/");
        } else {
            setError(result.message);
        }

        setLoading(false);
    }

    return (
        <form
            onSubmit={handleSubmit}
            className="mt-6 space-y-4"
        >
            <Input
                placeholder="Username"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
            />


            <Input
                type="email"
                placeholder="Email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
            />

            <Input
                type="password"
                placeholder="Password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
            />

            {error && (
                <Alert variant="destructive">
                    <AlertDescription>
                        {error}
                    </AlertDescription>
                </Alert>
            )}

            <Button
                className="w-full"
                disabled={loading}
                type="submit"
            >
                {loading ? "Creating account..." : "Register"}
            </Button>
        </form>
    );
}
