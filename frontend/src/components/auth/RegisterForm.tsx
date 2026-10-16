import { useState, type FormEvent } from "react";
import { Loader2 } from "lucide-react";
import { useAuth } from "@/hooks/useAuth";
import { getErrorMessage } from "@/api/client";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Alert, AlertDescription } from "@/components/ui/alert";

export function RegisterForm() {
    const { register } = useAuth();

    const [username, setUsername] = useState("");
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState("");

    async function handleSubmit(event: FormEvent) {
        event.preventDefault();

        setLoading(true);
        setError("");

        try {
            await register({ username, email, password });
        } catch (registerError) {
            setError(getErrorMessage(registerError));
            setLoading(false);
        }
    }

    return (
        <form onSubmit={handleSubmit} className="mt-6 space-y-4">
            <div className="grid gap-2">
                <Label htmlFor="register-username">Username</Label>
                <Input
                    id="register-username"
                    autoComplete="username"
                    placeholder="trader"
                    value={username}
                    onChange={(event) => setUsername(event.target.value)}
                />
            </div>

            <div className="grid gap-2">
                <Label htmlFor="register-email">Email</Label>
                <Input
                    id="register-email"
                    type="email"
                    autoComplete="email"
                    placeholder="you@example.com"
                    value={email}
                    onChange={(event) => setEmail(event.target.value)}
                />
            </div>

            <div className="grid gap-2">
                <Label htmlFor="register-password">Password</Label>
                <Input
                    id="register-password"
                    type="password"
                    autoComplete="new-password"
                    placeholder="••••••••"
                    value={password}
                    onChange={(event) => setPassword(event.target.value)}
                />
            </div>

            {error && (
                <Alert variant="destructive">
                    <AlertDescription>{error}</AlertDescription>
                </Alert>
            )}

            <Button className="w-full" disabled={loading} type="submit">
                {loading && <Loader2 className="animate-spin" />}
                {loading ? "Creating account..." : "Register"}
            </Button>
        </form>
    );
}
