# Trading Journal

# Technical Design Document (TDD)

**Version:** 1.0
**Status:** Draft

---

# 1. Overview

## Purpose

This document describes the technical architecture, technology stack, project structure, and design decisions for the Trading Journal application.

The objective is to build a modern, maintainable full-stack web application using the current .NET ecosystem while following industry best practices. The project also serves as a practical exercise for refreshing and strengthening modern .NET development skills.

---

# 2. Technology Stack

## Backend

| Technology            | Purpose                        |
| --------------------- | ------------------------------ |
| .NET 10               | Application Framework          |
| ASP.NET Core          | REST API & Static File Hosting |
| Entity Framework Core | ORM                            |
| SQL Server            | Relational Database            |
| JWT Authentication    | User Authentication            |
| FluentValidation      | Request Validation             |
| Serilog               | Structured Logging             |

---

## Frontend

| Technology     | Purpose                         |
| -------------- | ------------------------------- |
| React          | User Interface                  |
| TypeScript     | Type Safety                     |
| Vite           | Development Server & Build Tool |
| React Router   | Client-side Routing             |
| TanStack Query | Server State Management         |

---

## Development Tools

* Docker
* Docker Compose
* Git
* GitHub
* Swagger / OpenAPI

---

# 3. System Architecture

The application follows a client-server architecture.

```text
+-----------------------+
|   React Frontend      |
+-----------+-----------+
            |
            | HTTP
            |
+-----------v-----------+
| ASP.NET Core Backend  |
+-----------+-----------+
            |
            |
+-----------v-----------+
|   SQL Server Database |
+-----------------------+
```

The backend is responsible for:

* Authentication
* Business logic
* Data validation
* File uploads
* Statistics calculation
* Data persistence

The frontend is responsible for:

* User interface
* Form validation
* Data visualization
* User interaction

---

# 4. Deployment Model

## Development

During development:

* React runs using the Vite development server.
* ASP.NET Core hosts the REST API.
* Vite proxies API requests to the backend.

```text
React Dev Server
        │
        ▼
ASP.NET Core API
        │
        ▼
SQL Server
```

---

## Production

During publishing:

* React is compiled into static assets.
* The generated assets are copied into the ASP.NET application's `wwwroot` directory.
* ASP.NET Core serves both the frontend and backend.

The final deployment consists of:

* One executable
* One SQL Server database

No Node.js runtime is required after publishing.

---

# 5. Project Structure

```text
TradingJournal/

├── backend/
│   └── TradingJournal.Api/
│
├── frontend/
│   └── trading-journal-ui/
│
├── docs/
│
├── docker/
│
└── TradingJournal.sln
```

---

## Backend Structure

The backend follows a **Vertical Slice Architecture**, where each feature owns its API endpoints, business logic, validation, and persistence.

```text
TradingJournal.Api/

├── Features/
│   ├── Authentication/
│   ├── Dashboard/
│   ├── Trades/
│   ├── Import/
│   ├── Statistics/
│   ├── Tags/
│   ├── Notes/
│   └── Uploads/
│
├── Persistence/
│
├── Infrastructure/
│
├── Common/
│
└── Program.cs
```

This organization keeps related functionality together, improves maintainability, and minimizes unnecessary project complexity.

---

# 6. API Design

The backend exposes a RESTful API.

Primary endpoint groups include:

```text
/api/auth

/api/trades

/api/import

/api/dashboard

/api/statistics

/api/tags

/api/uploads
```

The frontend communicates exclusively through these endpoints.

---

# 7. Authentication

Authentication is implemented using:

* JWT Access Tokens
* Refresh Tokens

Passwords are securely hashed before storage.

All authenticated endpoints require a valid access token.

---

# 8. Data Storage

## SQL Server

SQL Server stores all application data, including:

* Users
* Trades
* Notes
* Tags
* Screenshot metadata
* User settings

Entity Framework Core is responsible for:

* Database access
* Migrations
* Entity mapping

---

## File Storage

Uploaded files include:

* CSV imports
* Trade screenshots

Files are initially stored locally using an abstraction layer that allows future migration to cloud storage if required.

---

# 9. Logging

Structured logging is implemented using Serilog.

The application logs:

* Authentication events
* Validation failures
* Import failures
* Unhandled exceptions
* Application startup and shutdown

---

# 10. Validation

Request validation is handled using FluentValidation.

Validation occurs before business logic execution to ensure:

* Consistent error responses
* Reduced duplicate validation code
* Improved maintainability

---

# 11. Error Handling

The application uses centralized exception handling.

Standard API responses include:

* 200 OK
* 201 Created
* 400 Bad Request
* 401 Unauthorized
* 403 Forbidden
* 404 Not Found
* 409 Conflict
* 500 Internal Server Error

Errors are returned using a consistent JSON response format.

---

# 12. Security

The application implements:

* Password hashing
* JWT authentication
* Authorization policies
* Input validation
* File upload validation
* HTTPS enforcement
* CORS configuration

---

# 13. Performance Considerations

The application should:

* Use asynchronous database operations.
* Project only required fields from queries.
* Use pagination for large result sets.
* Optimize Entity Framework queries.
* Avoid unnecessary database round trips.

Statistics are recalculated whenever trades are added, updated, deleted, or imported.

Given the expected scale of the application, all calculations are performed synchronously without background processing.

---

# 14. Testing Strategy

The project will include both unit and integration tests.

### Unit Tests

Testing:

* Statistics calculations
* Business logic
* Validation rules

### Integration Tests

Testing:

* API endpoints
* Authentication
* Database interactions
* Import functionality

---

# 15. Future Technical Enhancements

Potential future improvements include:

* Redis caching
* Hangfire for scheduled background jobs
* Azure Blob Storage or Amazon S3
* Full-text search
* Broker API integrations
* CI/CD pipeline
* Containerized cloud deployment

---

# 16. Architectural Principles

The project follows the following architectural principles:

* Feature-first organization using Vertical Slice Architecture.
* Clear separation between frontend and backend responsibilities.
* RESTful API design.
* Stateless backend using JWT authentication.
* Single deployable application with an embedded React frontend.
* Simplicity over unnecessary abstraction.
* Prefer built-in .NET capabilities before introducing additional frameworks.

The goal is to create a clean, maintainable codebase that reflects modern ASP.NET Core development practices while remaining approachable for future expansion.
