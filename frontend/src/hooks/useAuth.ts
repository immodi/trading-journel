import {
    createContext,
    useCallback,
    useContext,
    useMemo,
    useState,
} from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { getCurrentUser, login as loginRequest, register as registerRequest } from "@/api/auth";
import { TokenStorage } from "@/utils/tokenStorage";
import type { LoginRequest, RegisterRequest } from "@/types/requests/auth/requests";
import type { User } from "@/types/models";

interface AuthContextValue {
    user: User | null;
    isLoading: boolean;
    isAuthenticated: boolean;
    login: (request: LoginRequest) => Promise<void>;
    register: (request: RegisterRequest) => Promise<void>;
    logout: () => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function useAuth(): AuthContextValue {
    const context = useContext(AuthContext);

    if (!context) {
        throw new Error("useAuth must be used within an <AuthProvider>.");
    }

    return context;
}

export function useAuthProviderValue(): AuthContextValue {
    const queryClient = useQueryClient();
    const [hasSession, setHasSession] = useState(() => TokenStorage.has());

    const meQuery = useQuery({
        queryKey: ["me"],
        queryFn: getCurrentUser,
        enabled: hasSession,
        retry: false,
        staleTime: 5 * 60 * 1000,
    });

    const login = useCallback(async (request: LoginRequest) => {
        const token = await loginRequest(request);
        TokenStorage.set(token);
        setHasSession(true);
        await queryClient.fetchQuery({
            queryKey: ["me"],
            queryFn: getCurrentUser,
        });
    }, [queryClient]);

    const register = useCallback(async (request: RegisterRequest) => {
        const token = await registerRequest(request);
        TokenStorage.set(token);
        setHasSession(true);
        await queryClient.fetchQuery({
            queryKey: ["me"],
            queryFn: getCurrentUser,
        });
    }, [queryClient]);

    const logout = useCallback(() => {
        TokenStorage.remove();
        setHasSession(false);
        queryClient.clear();
    }, [queryClient]);

    return useMemo<AuthContextValue>(() => {
        const user = hasSession ? meQuery.data ?? null : null;

        return {
            user,
            isLoading: hasSession && meQuery.isLoading,
            isAuthenticated: user !== null,
            login,
            register,
            logout,
        };
    }, [hasSession, meQuery.data, meQuery.isLoading, login, register, logout]);
}

export { AuthContext };
