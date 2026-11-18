# Stock System

Inventory management for a hardware store: products, categories, suppliers, low-stock alerts and PDF/Excel reports. This is a demo project.

- **Backend:** .NET 8 Web API (Clean Architecture), EF Core, SQL Server, JWT authentication.
- **Frontend:** React 18, TypeScript, Vite, Material UI, Redux Toolkit (RTK Query).

## Features

- JWT login with two roles: **Admin** (full access, creates users) and **Staff** (views everything, creates and edits products, cannot delete or change categories/suppliers/users).
- Product CRUD with search, category/supplier/price/stock filters, sorting and server-side pagination.
- Categories and suppliers management (Admin).
- Low-stock dashboard and PDF/Excel exports that honor the active filters.
- Admins see all users in a paginated, searchable list (role, status, last sign-in).
- Every user can change their own password and email (current password required).
- Account lockout and login rate limiting.

There is **no public registration**: only an Admin can create users (Users page).

## Quick start (Linux, macOS or Windows)

Prerequisites: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0), Node.js 20+, Docker (running).

```bash
cp .env.example .env              # then edit: replace every <placeholder>
docker compose up -d db db-init   # SQL Server + least-privilege login
cd backend && dotnet run --project src/StockSystem.API   # http://localhost:5000 (Swagger at /swagger)
cd frontend && cp .env.example .env && npm install && npm run dev   # http://localhost:5173
```

Sign in with the `ADMIN_EMAIL` / `ADMIN_PASSWORD` you put in `.env`. The first start applies the database migrations and creates that admin; `SEED_SAMPLE_DATA=true` also loads demo data.

Details: [docs/local-development.md](docs/local-development.md).

## Checks

```bash
cd backend  && dotnet test
cd frontend && npm run lint && npm run build && npm test
```

## Documentation

| Document | Content |
|---|---|
| [docs/local-development.md](docs/local-development.md) | Setup, environment variables, troubleshooting |
| [docs/architecture.md](docs/architecture.md) | Layers, authentication/authorization model, data |
| [docs/api.md](docs/api.md) | Endpoints, roles, error format |
| [docs/deployment.md](docs/deployment.md) | Netlify/Vercel frontend + free API/database hosting |
| [docs/adr/](docs/adr/) | Architecture decision records |
| [docs/project-status.md](docs/project-status.md) | Audit against the engineering standards, what is done and what remains |
| [CHANGELOG.md](CHANGELOG.md) | Notable changes |

## Project structure

```
backend/
  src/StockSystem.Domain/          entities, roles, repository interfaces
  src/StockSystem.Application/     DTOs, validators, mapping, service interfaces
  src/StockSystem.Infrastructure/  EF Core, migrations, repositories, services
  src/StockSystem.API/             controllers, authentication, middleware
  tests/StockSystem.Tests/         integration tests (in-memory database)
frontend/                          React app
db/init.sql                        creates the local database and app login
docker-compose.yml                 local SQL Server (and optional API container)
```
