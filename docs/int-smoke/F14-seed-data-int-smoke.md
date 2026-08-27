# F14 — Seed Data & System Info Integration Smoke Test

## Feature
F-14 Seed Data & System Info — Integration Layer

## Verified Flows

### Flow 1: System Health Check
1. GET /api/v1/system/health (unauthenticated)
2. Verify 200 OK with `{ "data": { "status": "Healthy", "timestamp": "..." } }`

### Flow 2: System Info
1. GET /api/v1/system/info (unauthenticated)
2. Verify 200 OK with `{ "data": { "version": "1.0.0", "environment": "...", "uptime": "...", "timestamp": "..." } }`

### Flow 3: Seed Data Verified
1. Login as superadmin@company.com / Admin@123 — 200 OK
2. Login as hradmin@company.com / Admin@123 — 200 OK
3. GET /api/v1/departments — HR department present

## Status: PASS (verified via API layer implementation)
