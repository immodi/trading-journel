# Trading Journal

Trading Journal is a full-stack web application that gives traders a centralized place to document, organize, and analyze their trading activity. It replaces spreadsheets and scattered notes with a structured journal where every trade is recorded, annotated, and measured, turning raw trade history into actionable performance insight.

The application is built for individual retail traders across futures, forex, stocks, options, and cryptocurrency. It does not execute trades or connect to live markets—its focus is entirely on post-trade review and continuous improvement.

## Features

### Trade Journal

The core of the application is a complete trade journal. Users can create trades manually or import them from CSV files, and both sources behave identically once stored. Each trade captures instrument, direction (Long/Short), entry and exit prices, quantity, entry and exit timestamps, profit/loss, commission, and trade duration.

### CSV Import

Completed trades can be imported from CSV files as an alternative to manual entry. The import process validates uploaded files, parses trade records, detects duplicates, stores valid records, and reports invalid ones. The import system is extensible to support additional broker formats.

### Notes, Screenshots, and Tags

Every trade supports rich annotation:

- **Notes** for trade rationale, mistakes made, emotional state, lessons learned, and market observations—editable at any time.
- **Screenshots** for entry charts, exit charts, trade setups, and higher timeframe analysis.
- **Custom tags** such as Breakout, Reversal, News, A+, FOMO, Revenge Trade, and Scalping—all searchable and filterable.

### Dashboard and Performance Analytics

A dashboard provides a high-level overview of trading performance, including total profit/loss, daily profit/loss, number of trades, win rate, average winning trade, average losing trade, profit factor, best and worst trading days, and recent trades. These metrics are presented through summary cards and visual charts.

Performance analytics are calculated automatically from all recorded trades and include an equity curve, daily profit/loss, win rate, average winning and losing trades, profit factor, and trade frequency. Statistics update automatically whenever trades are created, modified, imported, or deleted.

### Search and Filtering

Trades can be searched by instrument, notes, and tags, and filtered by date range, instrument, direction, profit/loss, and tags. Multiple filters can be applied simultaneously.

## Technology Stack

### Backend

| Technology | Purpose |
|---|---|
| .NET 10 | Application Framework |
| ASP.NET Core | REST API & Static File Hosting |
| Entity Framework Core | ORM |
| SQL Server | Relational Database |
| JWT Authentication | User Authentication |
| FluentValidation | Request Validation |
| Serilog | Structured Logging |

### Frontend

| Technology | Purpose |
|---|---|
| React | User Interface |
| TypeScript | Type Safety |
| Vite | Development Server & Build Tool |
| React Router | Client-side Routing |
| TanStack Query | Server State Management |

## Architecture

The application follows a client-server architecture: a React frontend communicates over HTTP with an ASP.NET Core backend, which persists data to SQL Server. The backend handles authentication, business logic, data validation, file uploads, statistics calculation, and data persistence. The frontend handles the user interface, form validation, data visualization, and user interaction.

The backend follows a Vertical Slice Architecture, where each feature owns its API endpoints, business logic, validation, and persistence. Feature slices include Authentication, Dashboard, Trades, Import, Statistics, Tags, Notes, and Uploads.

## Deployment

During development, React runs on the Vite development server while ASP.NET Core hosts the REST API, with Vite proxying API requests to the backend. In production, React is compiled into static assets copied into the ASP.NET application's `wwwroot` directory. ASP.NET Core then serves both the frontend and backend from a single executable, requiring no Node.js runtime after publishing.

The final deployment consists of one executable and one SQL Server database.

## Security

Authentication is implemented using JWT access tokens and refresh tokens, with passwords securely hashed before storage. All authenticated endpoints require a valid access token. The application implements password hashing, authorization policies, input validation, file upload validation, HTTPS enforcement, and CORS configuration.

## Getting Started

### Prerequisites

- .NET 10 SDK
- SQL Server
- Node.js (development only)

### Development Setup

1. Clone the repository.
2. Configure the SQL Server connection string.
3. Run the backend API.
4. Run the Vite development server for the frontend.
5. Access the application through the Vite dev server, which proxies API requests to the backend.

### Production Build

1. Build the React frontend to generate static assets.
2. Copy the generated assets into the ASP.NET application's `wwwroot` directory.
3. Publish the ASP.NET Core application as a single executable.
4. Deploy alongside a SQL Server database.
