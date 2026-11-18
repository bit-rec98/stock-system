# Deployment

Target: a demo, hosted for free.

```
Browser ──► Netlify or Vercel (static React build)
   │
   └─ HTTPS + Bearer JWT ──► Render (API container) ──► Azure SQL Database (free offer)
```

Netlify and Vercel only host the frontend: they cannot run a long-lived ASP.NET Core process or SQL Server. See [ADR-002](adr/ADR-002-hosting.md).

> **Verification status:** the steps below were written from the project's configuration and from third-party summaries of each provider's free tier. None of them have been executed, and provider terms and limits change. Check each provider's current documentation and pricing before relying on them.

## 1. Database: Azure SQL Database (free offer)

1. Create an Azure subscription (a payment card is required) and create a database using the **free offer**. Choose the behavior *auto-pause until next month* when the free limits are reached, so it can never bill you.
2. Create a login/user for the app and give it rights on its own database only (the API applies EF migrations at startup, so it needs `db_owner` on that database):
   ```sql
   -- in the application database
   CREATE USER stock_app WITH PASSWORD = '<strong-password>';
   ALTER ROLE db_owner ADD MEMBER stock_app;
   ```
3. **Firewall (open item):** the API host must be allowed to connect. Render's outbound IP ranges must be added as Azure SQL firewall rules (see Render's documentation for the current ranges). Do not open the server to all addresses.
4. Connection string:
   `Server=tcp:<server>.database.windows.net,1433;Database=<db>;User ID=stock_app;Password=<password>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=60`

The serverless database pauses after about an hour without use and takes a while to resume; the API retries transient connection failures (`EnableRetryOnFailure`), but the first request after a pause can still be slow.

## 2. API: Render (Docker web service)

1. Create a Web Service from this repository, runtime **Docker**, root directory `backend` (uses `backend/Dockerfile`), health check path `/health`.
2. Environment variables (secrets only here, never in the repository):

   | Variable | Value |
   |---|---|
   | `ASPNETCORE_ENVIRONMENT` | `Production` |
   | `DB_CONNECTION_STRING` | the Azure SQL string above |
   | `JWT_KEY` | random, 32+ characters |
   | `JWT_ISSUER`, `JWT_AUDIENCE` | consistent values |
   | `CORS_ORIGINS` | the exact frontend URL, e.g. `https://your-site.netlify.app` |
   | `AllowedHosts` | the API host name, e.g. `your-api.onrender.com` (default `localhost` rejects other hosts) |
   | `ADMIN_EMAIL`, `ADMIN_PASSWORD` | initial admin (choose a strong password; change it after first login) |
   | `SEED_SAMPLE_DATA` | `true` for demo data, otherwise `false` |

3. The container listens on `$PORT` (set by Render). The free instance sleeps when idle and takes roughly a minute to wake: the first request after a quiet period is slow.

## 3. Frontend: Netlify (or Vercel)

**Netlify:** import the repository; `netlify.toml` already sets base `frontend`, command `npm run build`, publish `dist`, the SPA fallback and headers. Set the environment variable:

- `VITE_API_BASE_URL` = `https://your-api.onrender.com` (no trailing path; the app adds `/api`).

**Vercel:** import the repository with root directory `frontend`; `frontend/vercel.json` provides the SPA fallback and headers. Set the same `VITE_API_BASE_URL`.

`VITE_` variables are baked in at build time and are public: never put secrets in them. Redeploy after changing one.

After the frontend URL is known, set it in the API's `CORS_ORIGINS` and redeploy the API.

## 4. Smoke test

1. `GET https://<api>/health` returns `{"status":"ok"}`.
2. Open the frontend and sign in with `ADMIN_EMAIL`.
3. Create a Staff user on the Users page, sign in as that user: products are visible, delete and category/supplier editing are not.
4. Create/edit a product, export PDF and Excel.
5. Confirm an unauthenticated `GET https://<api>/api/products` returns `401`.

## Rollback

Frontend: redeploy a previous build from the Netlify/Vercel dashboard. API: redeploy the previous Render deploy. Migrations are forward-only; to undo a schema change, ship a new migration. Take a database backup/export before any release that includes a migration.

## Local production-like run

`docker compose --profile api up -d --build` builds the same Dockerfile and runs it against the local SQL Server container.
