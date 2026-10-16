import axios from "axios";
import { config } from "@/utils/config";
import { TokenStorage } from "@/utils/tokenStorage";
import type { ErrorResponse } from "@/types/responses/errorResponse";

export const api = axios.create({
    baseURL: `${config.apiUrl}/api`,
});

api.interceptors.request.use((request) => {
    const token = TokenStorage.get();

    if (token) {
        request.headers.Authorization = `Bearer ${token}`;
    }

    return request;
});

api.interceptors.response.use(
    (response) => response,
    (error: unknown) => {
        // An expired or revoked token should send the user back to the login
        // screen instead of leaving them staring at failed requests.
        if (axios.isAxiosError(error) && error.response?.status === 401) {
            TokenStorage.remove();

            if (window.location.pathname !== "/login") {
                window.location.assign("/login");
            }
        }

        return Promise.reject(error);
    }
);

/**
 * Turns anything thrown by axios into a message that is safe to show a user.
 */
export function getErrorMessage(error: unknown): string {
    if (axios.isAxiosError(error)) {
        const data = error.response?.data as Partial<ErrorResponse> | undefined;

        if (data?.message) return data.message;

        if (error.code === "ERR_NETWORK") {
            return "Could not reach the server. Make sure the API is running.";
        }

        return error.message;
    }

    if (error instanceof Error) return error.message;

    return "Something went wrong. Please try again.";
}
