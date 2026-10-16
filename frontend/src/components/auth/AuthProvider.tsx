import type { ReactNode } from "react";
import { AuthContext, useAuthProviderValue } from "@/hooks/useAuth";

export function AuthProvider({ children }: { children: ReactNode }) {
    const value = useAuthProviderValue();

    return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
