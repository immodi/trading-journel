import type { LoginRequest, RegisterRequest } from "@/types/requests/auth/requests";
import type { AuthResponse } from "@/types/responses/auth/responses";
import type { User } from "@/types/models";
import { api } from "./client";

export async function login(request: LoginRequest): Promise<string> {
    const response = await api.post<AuthResponse>("/auth/login", request);
    return response.data.token;
}

export async function register(request: RegisterRequest): Promise<string> {
    const response = await api.post<AuthResponse>("/auth/register", request);
    return response.data.token;
}

export async function getCurrentUser(): Promise<User> {
    const response = await api.get<User>("/auth/me");
    return response.data;
}
