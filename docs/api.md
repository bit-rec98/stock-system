# API reference

Base path `/api`. JSON in and out. Authenticated endpoints need `Authorization: Bearer <token>`. In Development, Swagger UI is available at `/swagger`; the schemas there are generated from the code and are the authoritative field reference.

Roles: **A** = Admin, **S** = Staff. "Any" = Admin or Staff.

## Auth

| Method | Path | Access | Notes |
|---|---|---|---|
| POST | `/auth/login` | anonymous, rate limited | Body `{ email, password }`. Returns `{ token, email, firstName, lastName, roles, expiration }`. `401` for wrong credentials, unknown or locked accounts (same message). |
| POST | `/auth/register` | **A** | Creates a user. Body `{ email, password, firstName, lastName, role }` where `role` is `Admin` or `Staff`. Returns `201 { email, firstName, lastName, role }`. Does not sign the new user in. |
| POST | `/auth/change-password` | any signed-in user, rate limited | Body `{ currentPassword, newPassword, confirmNewPassword }`. |
| POST | `/auth/change-email` | any signed-in user, rate limited | Body `{ newEmail, currentPassword }`. Changes the caller's own email, which is also the login name. Returns a **new session** (same shape as login) because the token carries the email; the old token keeps working until it expires but shows the old email. Failures are `400` (wrong password, same email, address already used, invalid format). Wrong passwords count toward the account lockout. |

Password policy: 8 to 128 characters with an uppercase letter, a lowercase letter and a digit.

## Users

| Method | Path | Access |
|---|---|---|
| GET | `/users` | **A**. Query: `pageNumber`, `pageSize` (max 100), `searchTerm` (email, first or last name, max 100 characters). Ordered by email. |

Response: the standard paged envelope (`items`, `totalCount`, `pageNumber`, `pageSize`, `totalPages`, `hasPreviousPage`, `hasNextPage`). Each item: `id`, `email`, `firstName`, `lastName`, `role`, `isActive`, `isLockedOut`, `createdAt`, `lastLoginAt`. Credentials and security stamps are never returned.

## Products

| Method | Path | Access |
|---|---|---|
| GET | `/products` | Any. Query: `pageNumber`, `pageSize` (max 100), `searchTerm`, `categoryId`, `supplierId`, `minPrice`, `maxPrice`, `isLowStock`, `isActive`, `sortBy` (`name`, `price`, `quantity`, `sku`, `category`, `createdAt`), `sortDescending` |
| GET | `/products/{id}` | Any |
| GET | `/products/low-stock` | Any |
| POST | `/products` | Any |
| PUT | `/products/{id}` | Any |
| DELETE | `/products/{id}` | **A** |
| GET | `/products/export/pdf`, `/products/export/excel` | Any. Same filters as the list; returns every matching product (not only one page) |

## Categories and suppliers

`GET /categories`, `GET /categories/paged`, `GET /categories/{id}` and the same three under `/suppliers` are open to Any role. `POST`, `PUT` and `DELETE` on both are **A** only. A category with products cannot be deleted.

## Other

| Method | Path | Access |
|---|---|---|
| GET | `/health` (no `/api` prefix) | anonymous; returns `{ "status": "ok" }` |

## Errors

| Status | Meaning |
|---|---|
| 400 | Validation failure (`errors` map of field to messages) or business rule (`{ message }`) |
| 401 | Missing, invalid or expired token; failed login |
| 403 | Authenticated but the role is not allowed |
| 404 | Resource not found |
| 429 | Too many requests to a credential endpoint |
| 500 | Unexpected error. Generic body; details are only in the server log |
