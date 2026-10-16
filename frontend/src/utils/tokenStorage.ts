const TOKEN_COOKIE = "session_token";

export class TokenStorage {
    static set(token: string, expires?: Date): void {
        let cookie = `${TOKEN_COOKIE}=${encodeURIComponent(token)}; Path=/; SameSite=Lax`;

        if (expires) {
            cookie += `; Expires=${expires.toUTCString()}`;
        }

        if (window.location.protocol === "https:") {
            cookie += "; Secure";
        }

        document.cookie = cookie;
    }

    static get(): string | null {
        const cookies = document.cookie.split("; ");

        for (const cookie of cookies) {
            const [name, value] = cookie.split("=");

            if (name === TOKEN_COOKIE) {
                return decodeURIComponent(value);
            }
        }

        return null;
    }

    static remove(): void {
        document.cookie = `${TOKEN_COOKIE}=; Path=/; Expires=Thu, 01 Jan 1970 00:00:00 GMT; SameSite=Lax`;
    }

    static has(): boolean {
        return this.get() !== null;
    }
}
