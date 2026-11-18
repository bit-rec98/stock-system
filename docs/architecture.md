# Architecture

## Overview

```
Browser (React SPA) ──HTTPS, Bearer JWT──► ASP.NET Core API ──► SQL Server
```

The frontend is a static build. It calls the API at `VITE_API_BASE_URL` (or through the Vite dev proxy locally). See [ADR-004](adr/ADR-004-api-routing.md).

## Backend layers (`backend/src`)

| Project | Responsibility | Depends on |
|---|---|---|
| `StockSystem.Domain` | Entities, `Roles`, repository and unit-of-work interfaces | none |
| `StockSystem.Application` | DTOs, FluentValidation validators, entity/DTO mapping, service interfaces, `Result<T>` | Domain |
| `StockSystem.Infrastructure` | EF Core `ApplicationDbContext`, migrations, repositories, services (products, categories, suppliers, auth, PDF/Excel export), seeding | Application, Domain |
| `StockSystem.API` | Controllers, JWT configuration, authorization policies, validation filter, rate limiting, security headers, composition root (`Program.cs`) | Application, Infrastructure |

Business logic lives in the services. Controllers translate HTTP to service calls. DTOs are used at the API boundary; entities are never returned.

## Request pipeline (`Program.cs`)

forwarded headers → (Development: Swagger / Production: HSTS + generic exception handler) → security headers → CORS → rate limiter → authentication → authorization → controllers. `ValidationFilter` runs the FluentValidation validator for each action argument and answers `400` with a `ValidationProblemDetails` body.

## Authentication and authorization

- **Login:** `POST /api/auth/login` returns a signed JWT (HS256; issuer, audience, lifetime and signature are validated, 1 minute clock skew). Passwords are hashed by ASP.NET Identity. After 5 failed attempts an account is locked for 15 minutes; locked and unknown accounts return the same message as a wrong password.
- **Roles and policies:**

  | Policy | Roles | Used for |
  |---|---|---|
  | `StaffOrAdmin` | Admin, Staff | Class-level default on products, categories and suppliers controllers (all reads) |
  | `AdminOnly` | Admin | User creation, product deletion, category and supplier create/update/delete |

  Staff can also create and update products and export reports.
- **Users:** only an Admin can create users (`POST /api/auth/register`), choosing `Admin` or `Staff`. Role changes and account disabling take effect when the token expires (`JWT_EXPIRATION_HOURS`, default 8), because the role is carried in the token.
- **Frontend:** hides controls the role cannot use (`useIsAdmin`). This is convenience only; the API enforces every rule. The token is kept in `localStorage` ([ADR-005](adr/ADR-005-token-storage.md)).

## Data

SQL Server via EF Core. The schema is managed by migrations in `StockSystem.Infrastructure/Data/Migrations` and applied at API startup. Startup also ensures the roles exist, creates the initial admin from environment variables, and optionally loads demo data (`SEED_SAMPLE_DATA`). Failure of any of these stops the API.

Main tables: `Products` (unique `SKU`), `Categories` (unique name), `Suppliers`, plus the ASP.NET Identity tables.

## Configuration

All configuration comes from environment variables (or a git-ignored `.env` for local runs). See [local-development.md](local-development.md).
