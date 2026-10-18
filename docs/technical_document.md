# Trading Journal

# Technical Design Document (TDD)

**Version:** 2.0
**Status:** Living document — describes the implemented system

> **How to read this document.** This document describes how the application is
> actually built. Where a design is not implemented yet it is tagged
> **[Planned]**. The companion [business requirements](./business_requirements.md)
> describe the product; the [entity diagram](./entity_diagram.png) shows the
> schema.

---

# 1. Overview

## Purpose

This document describes the technical architecture, technology stack, project structure, data model, API, and design decisions for the Trading Journal application.

The objective is to build a modern, maintainable full-stack web application using the current .NET ecosystem while following industry best practices. The project also serves as a practical exercise for refreshing and strengthening modern .NET development skills.

## Design goals

* **One deployable artifact.** The published app is a single self-contained executable that serves both the API and the compiled SPA.
* **A one-way dependency graph.** The backend is strictly layered; inner layers never reference outer ones.
* **The server owns the rules.** Authentication, validation, authorization, and profit/loss calculation all live in the API so every client shows identical numbers.
* **A thin, typed frontend.** The SPA presents and filters data; it does not reimplement business rules.
* **Zero external services to run.** Persistence is an embedded SQLite file and configuration is file/environment based — no database server, no cache, no message broker.

---

# 2. Technology Stack

## Backend

| Technology | Version | Purpose |
| --- | --- | --- |
| .NET | 10 | Runtime & application framework |
| ASP.NET Core | 10 | REST API & static file hosting |
| Entity Framework Core | 10 | ORM & migrations |
| SQLite | (embedded) | Relational database |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 10 | JWT bearer authentication |
| ASP.NET Core Identity password hasher | 10 | PBKDF2 password hashing |
| `Microsoft.AspNetCore.OpenApi` | 10 | OpenAPI document (development only) |
| Built-in `ILogger` | — | Console + debug logging |

## Frontend

| Technology | Purpose |
| --- | --- |
| React 19 | User interface |
| TypeScript | Type safety |
| Vite | Development server & build tool |
| React Router 7 | Client-side routing |
| TanStack Query 5 | Server state management |
| Tailwind CSS v4 + Base UI | Styling & accessible primitives |
| axios | HTTP client |
| lucide-react | Icons |

## Tooling

* xUnit + Moq + FluentAssertions — backend tests
* GitHub Actions — continuous integration
* PlantUML — source for the entity diagram (`docs/entity_diagram.uml`)

## Deliberately not used

These appear in the design's history but are intentionally out of scope for the
current release:

* **SQL Server / a server database** — SQLite keeps deployment to a single file.
* **Serilog** — the built-in logger is sufficient for a single-user app.
* **FluentValidation** — create requests rely on required members and update requests are partial; business rules live in services (see [§8](#8-validation--error-handling)).
* **Docker** — a single self-contained binary is simpler to ship self-hosted.

---

# 3. System Architecture

The application follows a client-server architecture: a React SPA talks to an ASP.NET Core API over JSON.

```text
+-----------------------+
|   React SPA (browser) |
+-----------+-----------+
            |  JSON over HTTPS
            v
+-----------------------+
|   ASP.NET Core API    |
|   auth · validation   |
|   P/L · persistence   |
|   SPA hosting         |
+-----------+-----------+
            |
            v
+-----------------------+
|   SQLite (db.db)      |
+-----------------------+
```

The backend is responsible for:

* Authentication & authorization
* Business logic and validation
* Profit/loss and commission calculation
* Data persistence
* Serving the compiled SPA

The frontend is responsible for:

* User interface and forms
* Client-side search, filtering, and sorting
* Data visualization (statistics and the equity curve)
* Theme handling

## Backend layering

The backend is split into four projects with a strict, one-way dependency direction:

```text
Domain          entities & enums                 (no dependencies)
   ▲
Application     DTOs, requests, mappers,         (depends on Domain)
                service & repository interfaces
   ▲
Infrastructure  EF Core, repositories, services, (depends on Application + Domain)
                JWT, migrations
   ▲
Api             controllers, exception handling, (depends on Infrastructure)
                composition root, wwwroot
```

* **Domain** — the `User` and `Trade` entities and the `Direction` enum. Pure data, no dependencies.
* **Application** — DTOs (`TradeDto`, `UserDto`), request models, mappers, and the `I*Service` / `I*Repository` interfaces. Defines the contracts inner code fulfils.
* **Infrastructure** — `AppDbContext`, EF Core configurations and migrations, the repositories and services that implement the Application interfaces, password hashing, and JWT issuing.
* **Api** — controllers, the global exception handler, authentication/authorization wiring, and `Program.cs` (the composition root and the SPA host).

## Frontend structure

```text
src/
├── api/         axios clients (client, auth, markets, trades)
├── hooks/       context + server state (useAuth, useTheme, useMarkets, useTrades)
├── lib/         pure helpers (analytics, format, datetime, trade import)
├── components/  layout, dashboard, markets, trades, theme, ui primitives
├── pages/       route screens (Auth, Dashboard, Markets, Trades, NotFound)
├── router/      route table
├── types/       request/response & model types
└── utils/       config, token storage
```

Providers are split from their context so React Fast Refresh stays reliable:
`components/**/XProvider.tsx` renders the provider, while `hooks/useX.ts` owns
the context, the `useX` hook, and the `useXProviderValue` logic.

---

# 4. Project Structure

```text
TradingJournal/
├── backend/
│   ├── backend.sln
│   ├── Domain/                 entities & enums
│   ├── Application/            DTOs, requests, interfaces, mappers
│   ├── Infrastructure/         EF Core, repositories, services, JWT, migrations
│   ├── Api/                    controllers, exception handling, Program.cs
│   ├── Infrastructure.Tests/   unit tests
│   └── Api.Tests/              test project scaffold
├── frontend/
│   ├── src/
│   ├── .env.development        dev API URL (committed)
│   └── index.html              anti-flash theme script
├── docs/                       requirements, technical doc, entity diagram
└── .github/workflows/          CI
```

---

# 5. API Design

The backend exposes a RESTful, JSON API from controllers under `/api`. Resource
names are singular (`/api/trade`, not `/api/trades`).

## Endpoints

| Method | Endpoint | Auth | Description |
| --- | --- | --- | --- |
| `GET` | `/` | No | In production: serves the SPA entry point. In development: a hint to run the SPA separately. |
| `POST` | `/api/auth/register` | No | Create an account; returns a JWT. `409` if the email or username is taken. |
| `POST` | `/api/auth/login` | No | Exchange email + password for a JWT. |
| `GET` | `/api/auth/me` | Yes | The signed-in user's profile. |
| `GET` | `/api/market` | Yes | List the caller's markets. |
| `GET` | `/api/market/{id}` | Yes | Get one of the caller's markets. |
| `POST` | `/api/market` | Yes | Create a market. `409` if the name is taken. |
| `PUT` | `/api/market/{id}` | Yes | Update a market. |
| `DELETE` | `/api/market/{id}` | Yes | Delete a market. `409` if it still has trades. |
| `GET` | `/api/trade` | Yes | List the caller's trades. |
| `GET` | `/api/trade/{id}` | Yes | Get one of the caller's trades. |
| `POST` | `/api/trade` | Yes | Create a trade. |
| `POST` | `/api/trade/import` | Yes | Bulk-import trades into one of the caller's markets. |
| `PUT` | `/api/trade/{id}` | Yes | Update a trade. |
| `DELETE` | `/api/trade/{id}` | Yes | Delete a trade. |
| `GET/POST/PUT/DELETE` | `/api/user[...]` | Yes | User administration (see the note below). |

> **Note.** The `UserController` offers list/get/create/update/delete over users
> and is reachable by any authenticated user. The app is distributed per user
> (single-user instances), so no admin role is planned.

## Conventions

* All trade and market endpoints operate **only** on the caller's own data.
* Errors use a consistent JSON body: `{ "code": <int>, "message": "<text>" }`.
* Unknown `/api/*` routes return that same JSON `404` rather than the SPA shell.
* Non-API routes fall back to the SPA `index.html` so deep links work.

---

# 6. Authentication & Authorization

## Tokens

Authentication is stateless and JWT-based:

* **Algorithm:** HMAC-SHA256 (HS256).
* **Lifetime:** 12 hours.
* **Claims:** `sub` (user id), `unique_name` (username), `role`, `jti`.

The bearer handler sets `MapInboundClaims = false` so claim names stay exactly as
issued (`sub`, not a WS-Federation URI). Issuer, audience, lifetime, and signing
key are all validated.

## Client handling

The SPA stores the token in a `session_token` cookie (SameSite=Lax; `Secure`
over HTTPS) and attaches it as `Authorization: Bearer <token>` via an axios
request interceptor. A `401` response clears the token and redirects to `/login`.

## Authorization

Every trade query is scoped by the user id from the `sub` claim. A trade that
belongs to another user is reported as **not found** (the API never reveals the
existence of another user's records).

## Signing key

At startup the key is resolved up front so misconfiguration fails fast:

* If `JwtSettings:Secret` is configured it must be **≥ 32 bytes**, otherwise startup throws.
* If it is empty, a 48-byte key is generated and persisted to `jwt-secret.key`
  next to the binary, then reused on later starts (keeps issued tokens valid
  across restarts). Set `JwtSettings__Secret` to supply your own.

---

# 7. Data Model & Storage

## Entities

```text
User
  Id            int          PK
  Username      string(50)   unique
  Email         string(255)  unique
  PasswordHash  string(255)
  CreatedAt     datetime (UTC)
  Trades        Trade[]      1 → many (cascade delete)
  Markets       Market[]     1 → many (cascade delete)

Market
  Id            int          PK
  UserId        int          FK → User.Id
  Name          string(100)  unique per user
  Currency      string(3)    ISO 4217
  CreatedAt     datetime (UTC)
  UpdatedAt     datetime (UTC)
  Trades        Trade[]      1 → many (delete restricted)

Trade
  Id                int          PK
  UserId            int          FK → User.Id
  MarketId          int          FK → Market.Id
  Instrument        string(20)
  Direction         Direction    enum, stored as int
  Quantity          int
  EntryPrice        decimal(18,4)
  ExitPrice         decimal(18,4)
  EntryTime         datetime (UTC)
  ExitTime          datetime (UTC)
  CommissionPerUnit decimal(18,4)
  Commission        decimal(18,4)
  ProfitLoss        decimal(18,4)
  CreatedAt         datetime (UTC)
  UpdatedAt         datetime (UTC)

Direction       enum
  Long = 0, Short = 1, Sideways = 2
```

See the rendered [entity diagram](./entity_diagram.png) and its
[PlantUML source](./entity_diagram.uml).

## Persistence

* **EF Core** maps the entities through `IEntityTypeConfiguration` classes
  (`UserConfiguration`, `MarketConfiguration`, `TradeConfiguration`).
* **SQLite** is the database (`ConnectionStrings:Default` = `Data Source=db.db`).
  The file is created on first run. SQLite is intentional for the single-user,
  self-hosted distribution model.
* **Unique indexes** enforce unique `Username` and `Email`, plus a unique
  `(UserId, Name)` index on `Markets`.
* **Precision** on monetary columns is `(18,4)`; `Quantity` is an `int`.
* **Timestamps** are stored as UTC via a global `UtcDateTimeConverter` (and a
  nullable variant), so comparisons are timezone-safe.
* **Migrations** are applied automatically at startup (`Database.Migrate()`).
  `AddMarketsAndCommissionPerUnit` adds the `Markets` table and the
  `MarketId`/`CommissionPerUnit` columns, attaching any pre-existing trades to a
  per-user "Imported" market so no history is lost.
* **Delete behaviour** — deleting a user cascades to their markets and trades;
  deleting a market that still has trades is refused (`409 Conflict`) rather
  than silently discarding trade history (enforced in the service and by a
  `Restrict` foreign key).

## Profit/loss and commission

Profit/loss is **derived and stored** (not computed on read), so every client
sees the same value and reads stay cheap:

```text
pnl = direction == Long
        ? (ExitPrice - EntryPrice) * Quantity
        : (EntryPrice - ExitPrice) * Quantity

ProfitLoss = pnl - Commission
```

The client supplies a **per-side** commission on create, edit, or import; the
server is the source of truth and always derives the stored total. A round trip
is charged on both the entry and the exit, so the stored per-unit rate is the
per-side rate doubled:

```text
CommissionPerUnit = CommissionPerSide × 2
Commission        = CommissionPerUnit × Quantity
```

There is no hard-coded default rate. The frontend only previews the total.

---

# 8. Validation & Error Handling

## Validation

Validation is intentionally lightweight:

* **Create** requests declare their required fields with C# `required` members,
  which the JSON serializer enforces; a missing field yields `400 Bad Request`.
* **Update** requests are partial — every field is nullable and only the supplied
  fields are applied.
* **Business rules** are enforced in the service layer, e.g. duplicate
  username/email on registration and duplicate market names (→ `409`), access to
  a trade or market the user does not own (→ `404`), a market name/currency that
  fails validation or a negative commission per side (→ `400`), and deleting a
  market that still has trades (→ `409`).
* **Import** requests are validated per row (non-empty instrument, Long/Short
  direction, positive quantity, non-negative prices and commission, ordered
  timestamps) after the target market's ownership is checked, and before any
  trade is written; a malformed batch never leaves a partial import behind.

> **[Planned]** FluentValidation for richer, declarative request rules.

## Error handling

A single `ExceptionHandler` (registered via `AddExceptionHandler`) translates
unhandled exceptions into status codes and a uniform `ErrorResponse`:

| Exception | Status |
| --- | --- |
| `KeyNotFoundException` | `404 Not Found` |
| `UnauthorizedAccessException` | `401 Unauthorized` |
| `ConflictException` | `409 Conflict` |
| `ArgumentException` | `400 Bad Request` |
| anything else | `500 Internal Server Error` |

`404`/`400`/`409` messages are passed through; `500`s are logged with the request
method and path and returned as a generic message so internals never leak.

Standard API responses used by the app: `200 OK`, `401 Unauthorized`,
`404 Not Found`, `409 Conflict`, `400 Bad Request`, and `500 Internal Server
Error` (`201 Created` is available where a resource location is meaningful).

---

# 9. Configuration

Configuration comes from `appsettings*.json` and environment variables.

| Setting | Location | Notes |
| --- | --- | --- |
| `ConnectionStrings:Default` | `Api/appsettings*.json` | SQLite connection string |
| `JwtSettings:Secret` | config or `JwtSettings__Secret` | ≥ 32 bytes; auto-generated + persisted when empty |
| `JwtSettings:Issuer` / `JwtSettings:Audience` | `Api/appsettings*.json` | Token issuer/audience |
| `JwtSettings:Role` | `Api/appsettings*.json` | Default role claim (`User`) |
| `VITE_API_URL` | `frontend/.env.development` (dev) | API base URL; empty in production = same origin |
| `FrontendApiUrl` | MSBuild (`-p:FrontendApiUrl=…`) | Bakes a custom API URL into the published SPA |
| `SkipFrontendBuild` | MSBuild (`-p:SkipFrontendBuild=true`) | Skips the SPA build during publish |

`appsettings.Development.json` holds a development connection string and a
throwaway signing key, so `dotnet run` works with no setup.

---

# 10. Frontend Architecture

## Routing

`createBrowserRouter` with:

* `/login` — public authentication screen.
* `/` — dashboard, behind a `ProtectedRoute`.
* `/trades` — trade journal, behind the same guard.
* `*` — not-found screen.

`AppLayout` renders the authenticated shell (header, theme toggle, sign-out) for
the protected routes.

## State

* **TanStack Query** manages server state; `useTrades` owns the trade query and
  its mutations, and invalidates on write.
* **AuthProvider / useAuth** hold the session (token presence + current profile)
  and expose register/login/logout.
* **ThemeProvider / useTheme** hold the theme choice (see below).

## HTTP layer

`api/client.ts` creates an axios instance with `baseURL = ${config.apiUrl}/api`
and installs the bearer-token request interceptor and the `401` response
interceptor described in [§6](#6-authentication--authorization).

## Analytics

Statistics (`lib/analytics.ts`) are computed on the client from the loaded trade
list: total trades, net P/L, win rate, gross profit/loss, total commission,
average win/loss, profit factor, best/worst trade, and an equity curve
(cumulative P/L). The dashboard renders them as summary cards plus a
dependency-free SVG equity curve.

Monetary metrics are computed per market (`calculateStatsForMarket`): because
every trade in a market inherits that market's currency, trades only ever share
a monetary total or equity curve with trades in the same currency, so amounts in
different currencies (for example EGP and USD) are never added together. The
dashboard exposes a compact market selector (built from `getMarketSummaries`,
which derives the market list from the loaded trades — no extra API call) that
switches between an **All markets** view and a single market's panel. The All
markets view shows only currency-independent metrics from
`calculateOverallStats` (total trades, win rate, market/currency counts); a
selected market shows its net P/L, averages, commission, and equity curve in its
own currency. The selector updates the cards and curve without a page reload.
The application performs no currency
conversion. `vitest` unit tests in `lib/analytics.test.ts` assert that
cross-currency trades never contribute to the same monetary total or curve.

## Theming

Three modes — `light`, `dark`, `system` — persisted to `localStorage`
(`trading-journal-theme`) and reflected on `<html>` via the `dark` class. An
inline script in `index.html` applies the stored theme before first paint to
avoid a flash of the wrong theme; `system` follows live OS changes via
`matchMedia`.

---

# 11. Deployment Model

## Development

Two processes:

* The **Vite dev server** (`:5173`) serves the SPA with hot reload.
* The **API** (`:5067`) serves JSON. CORS is configured to allow exactly
  `http://localhost:5173`.

The SPA reads the API URL from `VITE_API_URL` in `frontend/.env.development`.

```text
Vite dev server (:5173) --HTTP--> ASP.NET Core API (:5067) --> SQLite (db.db)
```

## Production

`dotnet publish` produces a **single, self-contained executable** that serves
both the SPA and the API — no separate web server and no .NET runtime on the
target machine:

```bash
dotnet publish backend/Api/Api.csproj \
  -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true \
  -o ./publish
```

Runtime-specific publishes default `SelfContained` and `PublishSingleFile` to
`true`, so `-r <rid>` is enough.

How it fits together:

* The **`PublishFrontend`** MSBuild target runs `pnpm install` + `pnpm build` and
  adds `frontend/dist` to the publish payload under `wwwroot` (requires Node +
  pnpm on the build machine; skip with `-p:SkipFrontendBuild=true`).
* **`IncludeNativeLibrariesForSelfExtract`** bundles native libraries such as
  SQLite's, which a single-file publish would otherwise drop.
* The **content root is anchored to `AppContext.BaseDirectory`**, so
  `appsettings*.json` and `wwwroot` resolve next to the binary regardless of the
  working directory it is launched from.
* The SPA is built with **`VITE_API_URL` empty** so it calls the API on its own
  origin; override with `-p:FrontendApiUrl=https://api.example.com` for a split
  deployment.
* On first run the app creates `db.db` and, if no secret is configured,
  `jwt-secret.key` beside the executable.

Run it:

```bash
cd publish
./Api --urls http://0.0.0.0:8080     # or ASPNETCORE_URLS=http://0.0.0.0:8080
```

The SPA's compiled files are excluded from the single-file archive so they stay
as real files under `wwwroot` (served directly as static assets); the rest of
the app self-extracts on launch.

---

# 12. Testing & CI

## Backend tests

`Infrastructure.Tests` holds **70 xUnit tests** covering:

* `TradeService` — CRUD, per-user scoping, batch import, P/L and commission rules
* `MarketService` — CRUD, per-user scoping, duplicate names, safe deletion
* `TradeRepository` — persistence, lookups, and market associations
* `MarketRepository` — persistence and per-user queries
* `UserService` — user retrieval
* `UserRepository` — persistence, lookups, and cascade deletion
* `PasswordHasher` — hash/verify round-trips

They use SQLite in-memory for the repository tests, Moq for the service tests,
and FluentAssertions throughout.

## Frontend checks

`pnpm test` (Vitest unit tests), `pnpm lint` (ESLint), and `pnpm build`
(`tsc -b` type-check + Vite build). Unit tests live beside the logic as
`*.test.ts`; `src/lib/analytics.test.ts` covers the per-currency aggregation.

## Continuous integration

[`ci.yml`](../.github/workflows/ci.yml) runs three jobs on every push and pull
request:

1. **Backend** — restore, build the solution, and run the tests.
2. **Frontend** — install with pnpm, run the unit tests, lint, and build.
3. **Publish** — a self-contained single-file publish smoke test to catch
   packaging regressions.

---

# 13. Efficiency

* All database access is asynchronous.
* Profit/loss is computed once at write time and stored, so list/detail reads are
  simple projections.
* Per-user queries are driven by indexed keys and the unique user indexes.
* The SPA is served as static files; the API serves JSON only.
* Analytics run on the client over the fetched list, which is ideal for a
  personal journal.

---

# 14. Architectural Principles

* **Layered with one-way dependencies** — Domain ← Application ← Infrastructure ← Api.
* **Clear separation** between frontend and backend responsibilities.
* **RESTful** JSON API design.
* **Stateless** backend using JWT authentication.
* **The server owns business rules**; the client stays a presentation layer.
* **Single deployable application** with an embedded React frontend.
* **Simplicity over unnecessary abstraction** — prefer built-in .NET capabilities
  before adding frameworks.

The goal is a clean, maintainable codebase that reflects modern ASP.NET Core
development practices while remaining approachable for future expansion.

---

# 15. Future Technical Enhancements

* Import duplicate detection and support for additional export formats
* Trade notes, screenshots, and tags (file storage via an abstraction with an
  object-storage backend)
