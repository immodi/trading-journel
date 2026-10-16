# Trading Journal — Frontend

The React + TypeScript single-page app for the Trading Journal. For the full
project overview, architecture, and setup, see the [root README](../README.md).

## Quick start

```bash
pnpm install
pnpm dev                 # http://localhost:5173
```

The dev server expects the API to be running (see the root README). The API URL
comes from `frontend/.env.development` (`http://localhost:5067`) — leave
`VITE_API_URL` unset for production builds so the SPA uses its own origin.

## Scripts

| Command | Description |
|---|---|
| `pnpm dev` | Start the Vite dev server with HMR |
| `pnpm build` | Type-check (`tsc -b`) and build for production |
| `pnpm lint` | Run ESLint |
| `pnpm preview` | Preview the production build locally |

## Structure

```
src/
  api/          axios clients (auth, trades) + error helpers
  components/
    auth/       login/register forms, auth provider
    dashboard/  stat cards, equity curve
    layout/     app shell, protected route, page header
    theme/      theme provider and light/dark/system toggle
    trades/     trade table, form dialog, filters
    ui/         shadcn-style primitives built on Base UI
  hooks/        TanStack Query hooks for auth and trades, plus theming
  lib/          analytics, formatting, and datetime helpers
  pages/        route screens
  router/       route configuration
  types/        shared models and request/response types
```

## Conventions

- Server state is owned by TanStack Query; components stay presentational.
- All API access goes through `src/api`; the shared `api` instance attaches the
  JWT and redirects to `/login` on a `401`.
- Timestamps are sent to the API as UTC ISO strings and rendered in the
  browser's local timezone (`src/lib/datetime.ts`).
