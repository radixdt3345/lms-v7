# Leave Management System — Phase 1

**Stack:** C# 12 · .NET 8 · ASP.NET Core · PostgreSQL 15 · EF Core · Hangfire  
**Frontend:** React 17 · Vite · MUI v5 · Redux Toolkit + Redux-Saga · Axios · MSAL React  
**Auth:** JWT Bearer RS256 (24 h) + HttpOnly Secure refresh cookie (7 d) + Azure AD OAuth2  
**Branch strategy:** `feature/*` / `fix/*` → `dev` → `main` (squash merge only)

## Getting Started

```bash
# Backend
cd backend && dotnet restore && dotnet run --project src/LMS.Api

# Frontend
cd frontend && npm install && npm run dev
```

## Testing

```bash
# Backend unit tests
dotnet test --filter 'Category!=Integration'

# Integration tests (requires Docker)
docker-compose -f docker-compose.test.yml up -d
dotnet test --filter 'Category=Integration'

# Frontend unit tests
cd frontend && npm run test

# E2E (manual trigger only — runs against staging)
npx playwright test
```

## Environment Variables

Copy `.env.example` to `.env` and fill in values. Never commit real credentials.
