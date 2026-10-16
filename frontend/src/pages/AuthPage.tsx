import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { LoginForm } from "@/components/auth/LoginForm";
import { RegisterForm } from "@/components/auth/RegisterForm";

export default function AuthPage() {
    return (
        <div className="flex min-h-screen items-center justify-center bg-muted/30 p-6">
            <Card className="w-full max-w-md">
                <CardHeader>
                    <CardTitle className="text-center text-2xl">
                        Trading Journal
                    </CardTitle>
                </CardHeader>

                <CardContent>
                    <Tabs defaultValue="login">
                        <TabsList className="grid w-full grid-cols-2">
                            <TabsTrigger value="login">
                                Login
                            </TabsTrigger>

                            <TabsTrigger value="register">
                                Register
                            </TabsTrigger>
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
