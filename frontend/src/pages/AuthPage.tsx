import { Navigate } from "react-router-dom";
import { LineChart } from "lucide-react";
import { useAuth } from "@/hooks/useAuth";
import { FullPageSpinner } from "@/components/layout/FullPageSpinner";
import { ThemeToggle } from "@/components/theme/ThemeToggle";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { LoginForm } from "@/components/auth/LoginForm";
import { RegisterForm } from "@/components/auth/RegisterForm";

export default function AuthPage() {
    const { isAuthenticated, isLoading } = useAuth();

    if (isLoading) {
        return <FullPageSpinner label="Checking your session..." />;
    }

    if (isAuthenticated) {
        return <Navigate to="/" replace />;
    }

    return (
        <div className="relative flex min-h-screen items-center justify-center bg-muted/30 p-6">
            <div className="absolute top-4 right-4">
                <ThemeToggle />
            </div>

            <Card className="w-full max-w-md">
                <CardHeader className="items-center text-center">
                    <span className="mx-auto flex size-10 items-center justify-center rounded-xl bg-primary text-primary-foreground">
                        <LineChart className="size-5" />
                    </span>
                    <CardTitle className="text-2xl">Trading Journal</CardTitle>
                    <CardDescription>
                        Sign in to review your trades and performance.
                    </CardDescription>
                </CardHeader>

                <CardContent>
                    <Tabs defaultValue="login">
                        <TabsList className="grid w-full grid-cols-2">
                            <TabsTrigger value="login">Login</TabsTrigger>
                            <TabsTrigger value="register">Register</TabsTrigger>
                        </TabsList>

                        <TabsContent value="login">
                            <LoginForm />
                        </TabsContent>

                        <TabsContent value="register">
                            <RegisterForm />
                        </TabsContent>
                    </Tabs>
                </CardContent>
            </Card>
        </div>
    );
}
