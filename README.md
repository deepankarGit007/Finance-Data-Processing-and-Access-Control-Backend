# Finance Data Processing & Access Control Backend

A role-based finance dashboard backend built with **.NET 8 / ASP.NET Core Web API**, Entity Framework Core 8, and PostgreSQL.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Language | C# 12 / .NET 8 |
| Framework | ASP.NET Core 8 Web API |
| ORM | Entity Framework Core 8 |
| Database | PostgreSQL 16 (via Npgsql) |
| Migrations | EF Core Migrations |
| Auth | JWT Bearer (HMAC-SHA256) |
| Password Hashing | BCrypt.Net-Next (cost factor 12) |
| Validation | FluentValidation + Data Annotations |
| Testing | xUnit + WebApplicationFactory (in-memory) |
| API Docs | Swagger UI (Swashbuckle) at `/docs` |

---

## Project Structure

```
FinanceDataProcessing_Backend/
├── src/
│   └── FinanceBackend/
│       ├── Controllers/        # Route handlers (thin — delegate to services)
│       ├── Core/               # Enums, JwtService, PasswordService, RequireRoleAttribute
│       ├── Data/               # EF DbContext + Migrations
│       ├── DTOs/               # Request/Response data transfer objects
│       │   ├── Auth/
│       │   ├── Dashboard/
│       │   ├── Transactions/
│       │   └── Users/
│       ├── Middleware/         # Global exception handler
│       ├── Models/             # EF Core entity models
│       ├── Services/           # Business logic (interfaces + implementations)
│       │   └── Interfaces/
│       ├── Validators/         # FluentValidation rules
│       ├── Program.cs
│       └── appsettings.json
├── tests/
│   └── FinanceBackend.Tests/   # xUnit integration tests (in-memory DB)
├── scripts/
│   └── seed.sql                # Demo data
├── docker-compose.yml
├── .env.example
└── FinanceBackend.sln
```

---

## Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for PostgreSQL)
- `dotnet ef` CLI tool (see below)

### 1. Start PostgreSQL
```bash
docker compose up -d
```

### 2. Install EF CLI (once)
```bash
dotnet tool install --global dotnet-ef
```

### 3. Apply Database Migrations
```bash
cd src/FinanceBackend
dotnet ef database update
```

### 4. Run the API
```bash
dotnet run --project src/FinanceBackend
```

The API is now available at: **http://localhost:5000**
Swagger UI: **http://localhost:5000/docs**

### 5. (Optional) Load Seed Data
```bash
docker exec -i finance_postgres psql -U finance_user -d finance_db < scripts/seed.sql
```

---

## Demo Users (after seeding)

| Role    | Email                    | Password      |
|---------|--------------------------|---------------|
| Admin   | admin@finance.local      | Admin1234!    |
| Analyst | analyst@finance.local    | Analyst1234!  |
| Viewer  | viewer@finance.local     | Viewer1234!   |

---

## API Reference

### Authentication
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/v1/auth/login` | None | Login, returns JWT |
| GET  | `/api/v1/auth/me`    | Any  | Current user profile |

### Users
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET    | `/api/v1/users`        | Admin   | List all users |
| POST   | `/api/v1/users`        | Admin   | Create user |
| GET    | `/api/v1/users/{id}`   | Self/Admin | Get user by ID |
| PATCH  | `/api/v1/users/{id}`   | Admin   | Update role/status |
| DELETE | `/api/v1/users/{id}`   | Admin   | Deactivate user (soft) |

### Transactions
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET    | `/api/v1/transactions`       | Any  | List (viewers: own only; analysts/admins: all) |
| POST   | `/api/v1/transactions`       | Admin | Create transaction |
| GET    | `/api/v1/transactions/{id}`  | Any  | Get by ID |
| PATCH  | `/api/v1/transactions/{id}`  | Admin | Update |
| DELETE | `/api/v1/transactions/{id}`  | Admin | Soft delete |

**Filter params:** `type`, `category`, `startDate`, `endDate`, `page`, `pageSize`

### Dashboard
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/v1/dashboard/summary`     | Any          | Income, expenses, net balance |
| GET | `/api/v1/dashboard/by-category` | Analyst+     | Category-wise totals |
| GET | `/api/v1/dashboard/trends`      | Analyst+     | Monthly trends (`?months=6`) |
| GET | `/api/v1/dashboard/recent`      | Any          | Recent N transactions (`?count=10`) |

---

## Role-Based Access Control

| Action | Viewer | Analyst | Admin |
|--------|:------:|:-------:|:-----:|
| Login / view own profile | ✅ | ✅ | ✅ |
| View own transactions | ✅ | ✅ | ✅ |
| View all transactions | ❌ | ✅ | ✅ |
| Create/update/delete transactions | ❌ | ❌ | ✅ |
| Dashboard summary + recent | ✅ | ✅ | ✅ |
| Category totals + trends | ❌ | ✅ | ✅ |
| Create/manage users | ❌ | ❌ | ✅ |

Enforcement is done via `[Authorize(Roles = "...")]` attributes on controllers and service-level role checks (viewers scoped to own records in `TransactionService`).

---

## Running Tests

```bash
dotnet test
```

Tests use `WebApplicationFactory<Program>` with an in-memory SQLite database — no external services needed.

---

## Environment Configuration

Copy `.env.example` to `.env` and set your values. The app reads configuration via `appsettings.json` or environment variables (using ASP.NET Core's `__` hierarchy convention):

```
Jwt__SecretKey=your-secret-key-at-least-32-chars
ConnectionStrings__DefaultConnection=Host=localhost;...
```

---

## Design Decisions & Assumptions

1. **EF Core Global Query Filter** — `HasQueryFilter(t => !t.IsDeleted)` on `Transaction` ensures soft-deleted records are transparently excluded from all queries, without any code changes in services.

2. **Role hierarchy as integer** — `UserRole` enum values (Viewer=0, Analyst=1, Admin=2) allow numeric comparisons if needed, though role enforcement uses string-based policy attributes for clarity.

3. **Viewers see own transactions only** — enforced in `TransactionService.GetAllAsync` based on the `isAdminOrAnalyst` flag passed from the controller (derived from JWT claims at the HTTP layer).

4. **All user emails normalized to lowercase** — prevents duplicate-email bugs from case sensitivity.

5. **Transactions are never physically deleted** — `IsDeleted` flag + EF global filter. This preserves audit history and allows recovery.

6. **No refresh tokens** — kept out of scope as this is an assessment project. Access tokens expire in 60 minutes (configurable).

7. **BCrypt work factor 12** — balances security (slow enough to deter brute force) with acceptable response time (~250ms per login).

8. **Pagination clamped to 1–100** — prevents accidental unbounded queries.

9. **CORS is open for all origins in dev** — should be restricted to specific origins in production.

---

## Project Highlights

- **Clean architecture** — Controllers are thin route handlers; all business logic lives in Services; Data layer is purely EF + models.
- **Interface-based DI** — all services defined behind interfaces for testability and future extensibility.
- **Structured error responses** — global middleware maps exceptions to `{ code, detail }` JSON with correct HTTP status codes.
- **FluentValidation** — rich, composable validation rules with clear error messages (e.g., no future dates, positive amounts, strong password policy).
- **Swagger UI** — full interactive API explorer with JWT auth support at `/docs`.
