# syntax=docker/dockerfile:1

# ---------------------------------------------------------------------------
# 1. Frontend: build the React SPA with pnpm/Vite.
# ---------------------------------------------------------------------------
FROM node:22-slim AS frontend

WORKDIR /src/frontend

# Pin the same major version the project uses; the lockfile is committed.
RUN npm install --global pnpm@11

# Install dependencies first so this layer is cached until the lockfile changes.
COPY frontend/package.json frontend/pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile

COPY frontend/ ./
RUN pnpm build

# ---------------------------------------------------------------------------
# 2. Backend: publish the API and bundle the built SPA under wwwroot.
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY backend/ ./backend/
# The API's PublishFrontend target copies this into the publish payload.
COPY --from=frontend /src/frontend/dist ./frontend/dist

RUN dotnet restore backend/Api/Api.csproj
RUN dotnet publish backend/Api/Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    -p:SkipFrontendBuild=true

# ---------------------------------------------------------------------------
# 3. Runtime: small ASP.NET Core image serving the SPA + API on port 8080.
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080

# Keep the SQLite database on a volume so data survives restarts.
ENV ConnectionStrings__Default="Data Source=/app/data/db.db"

COPY --from=build /app/publish ./

# Run as the non-root "app" user that ships with the .NET images. /app stays
# writable so the app can create the database (and, when no JwtSettings__Secret
# is supplied, generate a signing key next to the binary).
RUN mkdir -p /app/data && chown -R app:app /app
USER app

EXPOSE 8080

ENTRYPOINT ["dotnet", "Api.dll"]
