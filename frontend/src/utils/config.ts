// The API base URL. When the SPA is served by the API itself (the published,
// self-contained app) this is left empty so requests go to the same origin.
// Point it at the API (via VITE_API_URL in .env.development or the build
// environment) when the frontend and API run on different origins.
const rawApiUrl = import.meta.env.VITE_API_URL ?? "";

export const config = {
    apiUrl: rawApiUrl.replace(/\/+$/, ""),
};
