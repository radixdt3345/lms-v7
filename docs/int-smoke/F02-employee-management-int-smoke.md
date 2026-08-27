# F-02 Employee Management — Integration Layer Smoke Test

**Issue:** #24 — [F-02] Employee Management — Integration Layer  
**Date:** 2026-08-26  
**Method:** Code-inspection smoke test (NuGet/runtime blocked in build environment)

---

## Scope

Verify DB + API + UI layers are correctly wired end-to-end by static code inspection:
- API reads from `LmsDbContext` via `EmployeeService` which references the schema from F02-DB
- Controller maps service results to `ApiResponse<T>` envelope
- Frontend `employeeApi.ts` reads `response.data.data` (double-unwrap for axios + ApiResponse)
- All routes registered in `Program.cs` (DI) and `App.tsx` (router)

---

## Inspection Results

### 1. DB → API Wiring

**Verified:** `EmployeeService.cs` constructor accepts `LmsDbContext` and `IAuditLogService`.

```csharp
// EmployeeService.cs
public sealed class EmployeeService(LmsDbContext db, IAuditLogService auditLog) : IEmployeeService
```

**Verified:** `LmsDbContext` has `DbSet<User>` which includes the F02-DB migration columns:
- `Phone` (varchar)
- `JobTitle` (varchar)
- `DateOfJoining` (date)
- `ReportingManagerId` (uuid FK → users.id, SET NULL)
- Navigation: `ReportingManager` (User?) and `DirectReports` (ICollection<User>)

**Migration confirmed:** `20260826100000_AddEmployeeProfileFields.cs` — Up/Down both present.

---

### 2. API → Controller Wiring

**Verified:** `EmployeesController` is decorated `[ApiController]`, `[Route("api/v1/employees")]`.

All 9 endpoints correctly registered:

| Method | Route | Auth | Service call |
|--------|-------|------|-------------|
| GET | `/api/v1/employees` | HR_ADMIN, SUPER_ADMIN | `ListAsync` |
| GET | `/api/v1/employees/me` | All auth | `GetMeAsync` |
| PUT | `/api/v1/employees/me` | All auth | `SelfEditAsync` |
| GET | `/api/v1/employees/team` | MANAGER, HR_ADMIN, SUPER_ADMIN | `GetTeamAsync` |
| GET | `/api/v1/employees/{id}` | HR_ADMIN, SUPER_ADMIN | `GetByIdAsync` |
| POST | `/api/v1/employees` | HR_ADMIN, SUPER_ADMIN | `CreateAsync` |
| PUT | `/api/v1/employees/{id}` | HR_ADMIN, SUPER_ADMIN | `UpdateAsync` |
| DELETE | `/api/v1/employees/{id}` | HR_ADMIN, SUPER_ADMIN | `DeactivateAsync` |
| POST | `/api/v1/employees/{id}/anonymise` | SUPER_ADMIN | `AnonymiseAsync` |

**Route ordering:** `me` and `team` literal routes registered before `{id:guid}` — no routing conflict.

**Verified ApiResponse<T> envelope:** Every non-204 endpoint returns:
```csharp
return Ok(new { data = dto });  // matches ApiResponse<T>
```

---

### 3. Controller → Frontend API Key Matching

**Backend EmployeeDto fields (camelCase in JSON):**
```
id, name, email, phone, role, status, jobTitle, dateOfJoining,
departmentId, departmentName, reportingManagerId, reportingManagerName,
createdAt, updatedAt
```

**Frontend EmployeeDto interface (employeeApi.ts):**
```typescript
id, name, email, phone, role, status, jobTitle, dateOfJoining,
departmentId, departmentName, reportingManagerId, reportingManagerName,
createdAt, updatedAt
```

✅ All field names match — no key mismatch risk.

---

### 4. Frontend API → Components Wiring

**Verified:** `employeeApi.ts` uses `response.data.data` on all GET/POST/PUT endpoints:
```typescript
const response = await axios.get<ApiResponse<EmployeeDto[]>>(BASE);
return response.data.data;  // axios .data = HTTP body; .data = ApiResponse<T> payload
```

**Verified:** `EmployeeManagementPage.tsx` calls `listEmployees()` and `listDepartments()`.  
**Verified:** `MyProfilePage.tsx` calls `getMe()` and `selfEdit()`.  
**Verified:** `MyTeamPage.tsx` calls `getTeam()`.

---

### 5. DI Registration

**Verified:** `Program.cs` contains:
```csharp
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
```

---

### 6. Router Registration

**Verified:** `App.tsx` contains:
- `/admin/employees` → `<EmployeeManagementPage>` (ProtectedRoute)
- `/profile` → `<MyProfilePage>` (ProtectedRoute)
- `/team` → `<MyTeamPage>` (ProtectedRoute)

**Verified:** `store/index.ts` registers `employeeReducer` under the `employee` key.

---

### 7. Auto-Promote / Auto-Demote Logic (AC-20, AC-21, AC-22)

**Inspected:** `EmployeeService.AutoPromoteIfNeededAsync`:
- Loads user by `managerId`, checks `Role == "EMPLOYEE"`, upgrades to `"MANAGER"`, saves.
- Called in `CreateAsync` and `UpdateAsync` when `ReportingManagerId` is set.

**Inspected:** `EmployeeService.AutoDemoteIfNeededAsync`:
- Counts active `DirectReports` for the manager.
- If count == 0, downgrades to `"EMPLOYEE"`, saves.
- Called after `UpdateAsync` (old manager) and `DeactivateAsync`.

**Inspected:** Manual demotion guard in `UpdateAsync`:
- If `req.Role == "EMPLOYEE"` and current role is `"MANAGER"` and direct reports count > 0 → returns `(null, "MANAGER_HAS_ACTIVE_REPORTS")`.
- Controller maps this to HTTP 409.

---

## Integration Smoke Test Verdict

| Check | Result |
|-------|--------|
| Migration columns match entity | ✅ |
| Service reads entity via DbContext | ✅ |
| Controller wraps all responses in ApiResponse<T> | ✅ |
| Frontend reads response.data.data | ✅ |
| DTO field names match frontend interface | ✅ |
| DI registration present | ✅ |
| Routes registered in App.tsx | ✅ |
| Auto-promote/demote logic inspected | ✅ |
| No bare array returns from API | ✅ |

**Overall: PASS (code-inspection)**

No runtime issues detected by static analysis. Full integration testing requires the staging environment (covered in F02-TEST and F02-E2E issues).
