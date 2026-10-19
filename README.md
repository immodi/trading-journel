# Trading Journal

[![CI](https://github.com/immodi/trading-journel/actions/workflows/ci.yml/badge.svg)](https://github.com/immodi/trading-journel/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-19-61DAFB?logo=react&logoColor=black)](https://react.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-6-3178C6?logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS-4-06B6D4?logo=tailwindcss&logoColor=white)](https://tailwindcss.com/)

A full-stack trade journal for retail traders. Log every trade, let the server
compute profit/loss and commission, and watch a performance dashboard — win
rate, profit factor, average win/loss, and an equity curve — update as you go.

The app is a complete vertical slice of a real product: a typed React SPA, a
layered ASP.NET Core API, JWT authentication with per-user data isolation, an
EF Core persistence layer with migrations, and a test suite wired into CI.

> Status: the authentication, trade-journal, and analytics core is implemented
> and covered by tests. JSON trade import is available; trade annotations
> (notes, screenshots, tags) are on the [roadmap](#roadmap).

---

## Features

- **Authentication** — register / login with JWT access tokens. Passwords are
  hashed with ASP.NET Core Identity's PBKDF2 hasher; every API call is
  authenticated and scoped to the signed-in user.
- **Markets** — create, edit, and delete markets. Each market carries a single
  currency that every trade in it inherits.
- **Trade journal** — create, edit, and delete trades with instrument,
  direction (Long/Short), quantity, point value, entry/exit prices, entry/exit
  times, and an association to a market.
- **Trade import** — bulk-add trades from a `TradeReplay`-style JSON export
  under a market you choose; every row inherits that market's currency and the
  export meta's point value and per-side commission. Invalid files and rows are
  rejected with a clear message before anything is written.
- **Server-side P/L** — profit/loss is calculated by the API from entry/exit
  prices, quantity, point value, and commission, so every client shows identical
  numbers. You supply a point value and a per-side commission on create, edit,
  or import and the server derives the total (`per side × 2 × quantity`); input
  is validated on both ends.
- **Dashboard** — net P/L, win rate, profit factor, average win, average loss,
  total trades, commission, plus a dependency-free SVG equity curve and a
  recent-trades table. All metrics are derived from your trade history.
- **Search & filtering** — search by instrument and filter by direction and
  date range; sort the table by instrument, exit time, or P/L.
- **Polished UI** — responsive layout, accessible shadcn-style components,
  loading skeletons, and empty states throughout.
- **Light & dark themes** — a persisted theme toggle (light / dark / system,
  following the OS by default) with no flash of the wrong theme on load.

## Tech stack

### Backend

| Technology | Purpose |
|---|---|
| .NET 10 / ASP.NET Core | REST API & static file hosting |
| Entity Framework Core 10 | ORM & migrations |
| SQLite | Database (swappable via EF Core providers) |
| JWT bearer authentication | Stateless auth |
| xUnit, Moq & FluentAssertions | Unit & integration tests |

### Frontend

| Technology | Purpose |
|---|---|
| React 19 | UI |
| TypeScript | Type safety |
| Vite | Dev server & build tool |
| React Router | Client-side routing |
| TanStack Query | Server state management |
| Tailwind CSS v4 + Base UI | Styling & accessible primitives |

## Architecture

Client-server: a React SPA talks to an ASP.NET Core API over JSON. The API owns
all business rules — authentication, validation, P/L calculation, and
persistence — so the frontend stays a thin presentation layer.

The backend is split into four projects with a strict dependency direction:

```
Domain        entities & enums       (no dependencies)
   ▲
Application   DTOs, requests,        (depends on Domain)
              service/repo interfaces
   ▲
Infrastructure persistence, services, (depends on Application + Domain)
              JWT, EF Core, migrations
   ▲
Api           controllers, exception  (depends on Infrastructure)
              handling, composition root
```

The frontend mirrors this with clear seams: `api/` (HTTP), `hooks/` (server
state), `lib/` (pure helpers such as analytics and formatting), `components/`
(presentation), and `pages/`.

### Current data model

The implemented schema is `Users`, `Markets`, and `Trades`: one user → many
markets and many trades, and each trade belongs to exactly one market whose
currency it inherits. A market that still has trades cannot be deleted (`409`).
The [entity diagram](./docs/entity_diagram.png) shows it in full. Planned
entities — tags, notes, screenshots, and import history — are described
in the [requirements](./docs/business_requirements.md) and tracked on the
roadmap.

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) 20.19+ (or 22.12+) and [pnpm](https://pnpm.io/)
- No database server required — SQLite is created automatically on first run.

### 1. Run the API

```bash
cd backend
dotnet restore
dotnet run --project Api
```

The API starts on <http://localhost:5067>. On startup it applies EF Core
migrations and creates `backend/Api/db.db`. Development configuration
(connection string and a throwaway JWT signing key) lives in
`Api/appsettings.Development.json`.

### 2. Run the frontend

```bash
cd frontend
pnpm install
pnpm dev
```

Open <http://localhost:5173>, register an account, and log your first trade.
During development the SPA talks to the API on `:5067` (see
`frontend/.env.development`).

Prefer containers? See [Docker](#docker) to build and run the whole app — SPA
and API — from a single image.

## Configuration

| Setting | Location | Notes |
|---|---|---|
| `ConnectionStrings:Default` | `backend/Api/appsettings*.json` | SQLite connection string |
| `JwtSettings:Secret` | `backend/Api/appsettings*.json` | ≥ 32 bytes; see below |
| `JwtSettings:Issuer` / `Audience` | `backend/Api/appsettings*.json` | Token issuer/audience |
| `VITE_API_URL` | `frontend/.env.development` | API base URL for dev; leave unset in production |

For production, provide the signing key as an environment variable so it never
lands in source control:

```bash
JwtSettings__Secret="<a long random secret>" dotnet run --project Api
```

If no secret is configured, the app generates a 48-byte key on first run and
persists it to `jwt-secret.key` next to the binary (reused on later starts). A
configured secret shorter than 32 bytes fails fast at startup.

## API reference

All trade endpoints require an `Authorization: Bearer <token>` header and only
operate on the caller's own data.

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/auth/register` | Create an account, returns a JWT |
| `POST` | `/api/auth/login` | Exchange credentials for a JWT |
| `GET` | `/api/auth/me` | Current user's profile |
| `GET` | `/api/market` | List the current user's markets |
| `GET` | `/api/market/{id}` | Get a single market |
| `POST` | `/api/market` | Create a market |
| `PUT` | `/api/market/{id}` | Update a market |
| `DELETE` | `/api/market/{id}` | Delete a market (`409` if it has trades) |
| `GET` | `/api/trade` | List the current user's trades |
| `GET` | `/api/trade/{id}` | Get a single trade |
| `POST` | `/api/trade` | Create a trade |
| `POST` | `/api/trade/import` | Bulk-import trades into a market (`{ marketId, pointValue, commissionPerSide, trades }`) |
| `PUT` | `/api/trade/{id}` | Update a trade |
| `DELETE` | `/api/trade/{id}` | Delete a trade |

Errors are returned as a consistent JSON body: `{ "code": 404, "message": "…" }`.
Unhandled exceptions are translated to the right status code by a global
exception handler.

## Testing

```bash
# Backend: 60 xUnit tests (services, repositories, password hashing)
cd backend
dotnet test

# Frontend: unit tests, type-check, lint, and production build
cd frontend
pnpm test
pnpm lint
pnpm build
```

Both suites run on every push and pull request via
[GitHub Actions](./.github/workflows/ci.yml).

## Project structure

```
backend/
  Domain/           entities, enums
  Application/      DTOs, requests, interfaces, mappers
  Infrastructure/   EF Core, repositories, services, JWT, migrations
  Api/              controllers, exception handling, Program.cs
  Infrastructure.Tests/  unit & integration tests
frontend/
  src/api/          HTTP clients
  src/hooks/        auth, theme, market & trade server state
  src/lib/          analytics, formatting, datetime helpers
  src/components/   layout, dashboard, markets, trades, ui primitives
  src/pages/        route screens
docs/               business requirements, technical doc, domain diagram
```

## Production build

`dotnet publish` produces a single, self-contained executable that serves both
the SPA and the API — no separate web server, no .NET runtime required on the
target machine:

```bash
dotnet publish backend/Api/Api.csproj \
  -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true \
  -o ./publish
```

(For any runtime-specific publish, `SelfContained` and `PublishSingleFile`
default to `true`, so `dotnet publish backend/Api/Api.csproj -c Release -r linux-x64`
is enough.)

How it fits together:

- The `PublishFrontend` MSBuild target runs `pnpm install` + `pnpm build` and
  adds the SPA to the publish payload under `wwwroot` (requires Node.js + pnpm
  on the build machine; skip with `-p:SkipFrontendBuild=true`).
- Native libraries such as SQLite are bundled for self-extraction, and the
  content root is anchored to the executable — so the app serves `wwwroot` no
  matter which directory you launch it from.
- The SPA is built with `VITE_API_URL` empty, so it calls the API on its own
  origin. For a split deployment, bake in a different URL with
  `-p:FrontendApiUrl=https://api.example.com`.
- Transfers are one executable, its `wwwroot`, and a SQLite file created on
  first run. SQLite is intentional: it keeps the app a single-file, per-user
  deployment.

Run it:

```bash
cd publish
./Api --urls http://0.0.0.0:8080     # or ASPNETCORE_URLS=http://0.0.0.0:8080
```

## Docker

The repository ships a multi-stage [`Dockerfile`](./Dockerfile) that builds the
SPA, publishes the API, and produces a runtime image that serves both on port
`8080`. Only Docker is required on the host — no .NET SDK or Node.js.

Build the image:

```bash
docker build -t trading-journal .
```

Run it, persisting the SQLite database on a named volume and supplying a signing
key:

```bash
docker run -d --name trading-journal \
  -p 8080:8080 \
  -e JwtSettings__Secret="<a long random secret>" \
  -v trading-journal-data:/app/data \
  trading-journal
```

Open <http://localhost:8080>.

### Docker Compose

For a one-command setup, [`docker-compose.yml`](./docker-compose.yml) builds the
same image and wires up the port, the signing key, and a named data volume:

```bash
cp .env.example .env          # then set JwtSettings__Secret in the new .env
docker compose up -d --build
```

Open <http://localhost:8080>. The journal is stored in the
`trading-journal-data` volume; stop with `docker compose down` (add `-v` to
delete the data as well). Compose reads the secret from `.env` (ignored by git)
or the exported environment, and refuses to start when it is unset.

Notes:

- **Signing key.** Pass `JwtSettings__Secret` (at least 32 bytes) so issued
  tokens stay valid across container rebuilds. Without it the app generates a
  key at `/app/jwt-secret.key`, which is discarded when the container is
  recreated.
- **Data.** The database lives at `/app/data/db.db`; mount a volume there (as
  above) to keep your journal across restarts. A disposable run can omit the
  volume: `docker run --rm -p 8080:8080 -e JwtSettings__Secret=... trading-journal`.
- **TLS.** The container serves plain HTTP on `:8080`. Run it behind a reverse
  proxy that terminates TLS (or put HTTPS at the edge); with no HTTPS port
  configured the app serves HTTP directly.
- **User.** The process runs as the non-root `app` user bundled with the .NET
  image.

Any app setting can be overridden with the usual `Section__Key` environment
variable syntax (for example `JwtSettings__Issuer` or
`ConnectionStrings__Default`); see [Configuration](#configuration).

## Roadmap

- Import duplicate detection and support for additional export formats
- Trade tags, notes, and chart screenshots

## Documentation

- [Business requirements](./docs/business_requirements.md)
- [Technical design](./docs/technical_document.md)
- [Domain diagram](./docs/entity_diagram.png)

## License

Released under the [MIT License](./LICENSE).

Built by [Ahmed Yasser (@immodi)](https://github.com/immodi).
