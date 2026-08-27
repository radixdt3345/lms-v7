# F-13 Audit Trail — Integration Smoke Test

**Issue:** #90  
**Date:** 2026-08-27  
**Method:** Code inspection (static analysis of implementation)

---

## 1. Immutability — AC-65

**Check:** `AuditLogsController` must not expose any mutating verbs.

**Finding:**
```csharp
// AuditLogsController.cs
[HttpPost]
[HttpPut]
[HttpDelete]
[HttpPatch]
public IActionResult RejectMutations() =>
    StatusCode(StatusCodes.Status405MethodNotAllowed);
```

- `[HttpGet]` is the only read path — returns `200 ApiResponse<PagedResult<AuditLogDto>>`
- All write verbs (`POST`, `PUT`, `DELETE`, `PATCH`) return `405 Method Not Allowed`
- No `[HttpPost]` / `[HttpPut]` / `[HttpDelete]` route maps to any data-modifying method
- `AuditLog` rows are only inserted via `IAuditLogService.LogAsync` (called by other services on mutation events) — never from the controller layer

**Result:** PASS — audit log endpoint is append-only.

---

## 2. Search / Filter Correctness

**Check:** All five filter parameters are wired through to the EF Core query.

**Finding in `AuditLogService.SearchAsync`:**

| Filter param       | EF Core predicate                                  |
|--------------------|----------------------------------------------------|
| `UserId`           | `.Where(a => a.ActorId == p.UserId)`               |
| `ActionType`       | `.Where(a => a.Action == p.ActionType)`            |
| `RecordType`       | `.Where(a => a.EntityType == p.RecordType)`        |
| `FromDate`         | `.Where(a => a.CreatedAt >= p.FromDate.Value)`     |
| `ToDate`           | `.Where(a => a.CreatedAt <= p.ToDate.Value)`       |

- Filters are applied only when the corresponding parameter is non-null/non-empty
- Results are ordered by `CreatedAt DESC` (most recent first)
- Pagination uses `.Skip((p.Page - 1) * p.PageSize).Take(p.PageSize)`
- `TotalCount` is computed with `CountAsync` before pagination (correct for server-side paging)

**Result:** PASS — all filters correctly propagate to the database query.

---

## 3. Write-on-Mutations (AuditLogService.LogAsync)

**Check:** `LogAsync` writes a new `AuditLog` row and persists it atomically.

**Finding:**
```csharp
public async Task LogAsync(...)
{
    _db.AuditLogs.Add(new AuditLog { ... CreatedAt = DateTime.UtcNow });
    await _db.SaveChangesAsync(ct);
}
```

- `AuditLog` entity has no `UpdatedAt` or soft-delete column — it is truly immutable once inserted
- `IAuditLogService` is registered as `Scoped` in `Program.cs` — correct lifetime for EF Core `DbContext`

**Result:** PASS — mutations are logged atomically.

---

## 4. Role Authorization

**Check:** Only `HR_ADMIN` and `SUPER_ADMIN` may call `GET /api/v1/audit-log`.

**Finding:**
```csharp
[Authorize(Roles = "HR_ADMIN,SUPER_ADMIN")]
public sealed class AuditLogsController : ControllerBase
```

- Class-level `[Authorize]` attribute — no endpoint can be reached unauthenticated
- `EMPLOYEE` and `MANAGER` roles will receive `403 Forbidden`

**Result:** PASS — role guard is correctly placed at the controller level.

---

## 5. Response Shape

**Check:** Response matches `ApiResponse<PagedResult<AuditLogDto>>` wrapper.

**Finding:**
```csharp
return Ok(ApiResponse<PagedResult<AuditLogDto>>.Ok(result));
```

`PagedResult<AuditLogDto>` properties: `Items`, `TotalCount`, `Page`, `PageSize`  
`AuditLogDto` properties: `Id`, `ActorUserId`, `ActorName`, `ActionType`, `RecordType`, `RecordId`, `OldValue`, `NewValue`, `IpAddress`, `Timestamp`

Frontend reads: `response.data.data.items` (axios `.data` = HTTP body, `ApiResponse .data` = payload) — verified in `auditLogApi.ts`.

**Result:** PASS — response shape is consistent with API convention.

---

## Summary

| Check                    | Result |
|--------------------------|--------|
| Immutability (AC-65)     | PASS   |
| Search filter coverage   | PASS   |
| Write-on-mutation        | PASS   |
| Role authorization       | PASS   |
| Response shape           | PASS   |

All integration smoke checks passed via code inspection.
