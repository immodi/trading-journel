import type { LoginRequest, RegisterRequest } from "../types/requests/auth/requests";
import type { AuthResponse } from "../types/responses/auth/responses";
import type { ErrorResponse } from "../types/responses/errorResponse";
import { api } from "./client";

type AuthRequestResponse = AuthResponse | ErrorResponse;

export async function login(
    request: LoginRequest
): Promise<AuthRequestResponse> {
    const response = await api.post<AuthRequestResponse>("/auth/login", request);
    return response.data;
}

export async function register(
    request: RegisterRequest
): Promise<AuthRequestResponse> {
    const response = await api.post<AuthRequestResponse>("/auth/register", request);
    return response.data;
}
