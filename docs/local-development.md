# Local development

## Requirements

| Tool | Version | Notes |
|---|---|---|
| .NET SDK | 8.x | SDK 10 also builds this project (it targets `net8.0`) |
| Node.js | 20+ (22 used by CI/Netlify config) | |
| Docker | with the daemon running | Runs SQL Server. LocalDB is Windows-only and is no longer used. |

## 1. Environment file

```bash
cp .env.example .env
```

Edit `.env` in the repository root and replace every `<placeholder>`:

| Variable | Purpose |
|---|---|
| `SA_PASSWORD` | SQL Server administrator password. Used only by the `db-init` step. 8+ characters with upper, lower, digit and symbol. |
| `APP_DB_PASSWORD` | Password for the `stock_app` login the API uses. Must also appear in `DB_CONNECTION_STRING`. |
| `DB_CONNECTION_STRING` | Connection string used when the API runs on your machine (`Server=localhost,1433;...`). |
| `JWT_KEY` | Random secret, at least 32 characters (`openssl rand -base64 48`). The API refuses to start otherwise. |
| `JWT_ISSUER`, `JWT_AUDIENCE` | Token issuer and audience (any consistent values). |
| `JWT_EXPIRATION_HOURS` | Token lifetime, default 8. |
| `CORS_ORIGINS` | Comma-separated exact frontend origins. |
| `ADMIN_EMAIL`, `ADMIN_PASSWORD` | Initial admin, created on first start if it does not exist. |
| `SEED_SAMPLE_DATA` | `true` loads demo categories, suppliers and products into an empty database. |

`.env` is git-ignored. Never commit it. The API finds it by searching the working directory and its parents; Docker Compose reads it from the repository root.

## 2. Database

```bash
docker compose up -d db db-init
```

`db` starts SQL Server on `127.0.0.1:1433`. `db-init` creates the `StockSystemDb` database and the `stock_app` login (see `db/init.sql`) and then exits. The data lives in the `sqldata` Docker volume; `docker compose down -v` deletes it.

## 3. API

```bash
cd backend
dotnet run --project src/StockSystem.API
```

Listens on `http://localhost:5000`; Swagger is at `/swagger` in Development. On start it applies migrations, creates the roles and the admin, and logs `Database migrated and seeded`.

To run the API in a container instead (production-like): `docker compose --profile api up -d --build`.

## 4. Frontend

```bash
cd frontend
cp .env.example .env     # leave VITE_API_BASE_URL empty: Vite proxies /api to localhost:5000
npm install
npm run dev              # http://localhost:5173
```

## Tests and checks

```bash
cd backend  && dotnet test
cd frontend && npm run lint && npm run build && npm test
```

Backend tests run the real API pipeline against an in-memory database; they need no Docker.

## Database migrations

After changing entities or the model configuration, from `backend/`:

```bash
dotnet tool install --global dotnet-ef --version 8.*   # once
dotnet ef migrations add <Name> \
  --project src/StockSystem.Infrastructure \
  --startup-project src/StockSystem.Infrastructure \
  --output-dir Data/Migrations
```

Review the generated migration before committing. It is applied automatically at startup.

## Troubleshooting

| Symptom | Cause |
|---|---|
| API exits with `JWT_KEY must be set and at least 32 bytes long` | `.env` missing or not found; set the variable |
| API exits with a SQL connection error | Database not running or the password in `DB_CONNECTION_STRING` differs from `APP_DB_PASSWORD`. Check `docker compose ps` and `docker compose logs db db-init` |
| Browser shows CORS errors | `CORS_ORIGINS` must contain the exact origin (scheme, host and port) of the frontend |
| `400 Invalid Hostname` | `AllowedHosts` in `appsettings.json` is `localhost`; set the `AllowedHosts` environment variable when the API is served under another host name |
| Login always fails after several attempts | The account is locked for 15 minutes |
