# Role-Based POS Authentication (Loyverse Model)

> **Status:** Parked — for future implementation
> **Date:** 2026-03-24
> **Context:** Shift from per-cashier Clerk accounts to owner-login + employee PIN switching

---

## Overview

Replace the current model (every cashier has a Clerk account) with a Loyverse-style approach:

1. **Only the owner/admin logs into POS** with Clerk (one login per device)
2. **Employees are assigned Roles** with configurable access levels and permissions
3. **Employees with POS access get a PIN** (4–6 digits, set by admin)
4. **On POS, employees switch in/out via PIN** — no Clerk sign-out/sign-in needed
5. **Shifts and transactions are attributed to the Employee**, not the Clerk User

---

## Current vs. Loyverse Model

| Aspect | Current | Loyverse-style |
|---|---|---|
| POS login | Each cashier has own Clerk account | Owner logs in once per device |
| Operator identity | JWT `UserId` (Clerk user) | Employee PIN |
| Shift/Transaction attribution | `User.Id` | `Employee.Id` |
| PIN purpose | Lock/unlock screen | **Identify the operator** |
| PIN lives on | `User.PinHash` | `Employee.PinHash` |
| Role/permissions | Single `org_role` from Clerk | Custom `Role` entity with granular permissions |
| Cashier onboarding | Create Clerk account + invite | Just create Employee + assign Role + set PIN |

---

## New Domain Concepts

### Role Entity

```csharp
public sealed class Role : IAuditableEntity
{
    public string Id { get; set; }
    public string TenantId { get; set; }
    public string Name { get; set; }          // e.g., "Cashier", "Manager", "Attendant"
    public string? Description { get; set; }
    public AccessLevel AccessLevel { get; set; } // AdminOnly, PosOnly, AdminAndPos
    public List<string> Permissions { get; set; } // JSON column or separate table
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum AccessLevel
{
    AdminOnly = 1,
    PosOnly = 2,
    AdminAndPos = 3,
}
```

### Employee Changes

```csharp
// Add to Employee entity:
public string? RoleId { get; set; }   // FK to Role
public Role? Role { get; set; }
public string? PinHash { get; set; }  // Moves FROM User TO Employee
```

### User Changes

```csharp
// Remove from User entity:
// public string? PinHash { get; set; }  ← DELETE (moves to Employee)
```

---

## POS Auth Flow

```
Current:
  Clerk Login → TenantContext.UserId → Open Shift → Operate

New:
  Owner Clerk Login → PIN Entry Screen → Employee identified
  → "Active Operator" set → Open Shift → Operate
```

- Owner's Clerk session provides **tenant context only** (data isolation)
- The PIN screen becomes the **primary operator switch**, not just a lock
- Switching operators = enter a different employee's PIN
- Lock screen and operator-switch screen merge into one concept

---

## Refactors Required

### Domain Layer

| Entity | Change |
|---|---|
| `Role` (NEW) | Name, AccessLevel, Permissions, TenantId |
| `Employee` | Add `RoleId` FK, add `PinHash`, remove `UserId` FK |
| `User` | Remove `PinHash` |
| `CashierShift` | `CashierId` changes from `User.Id` → `Employee.Id` |
| `Transaction` | `CashierId` changes from `User.Id` → `Employee.Id` |
| `ShiftSettings` | `MaxPinAttempts` stays (applies to employee PIN now) |

### Application Layer

| Area | Change |
|---|---|
| `TenantContext` | Add `OperatorEmployeeId` (set by PIN verification, not JWT) |
| `SetUserPinCommand` | Rename → `SetEmployeePinCommand`, target Employee not User |
| `VerifyPinCommand` | Changes to look up Employee by PIN instead of User |
| Shift gate checks | Query by `EmployeeId` instead of `UserId` |
| `OpenShiftCommand` handler | Use `OperatorEmployeeId` instead of `tenantContext.UserId` |
| `CreateTransactionCommand` handler | Use `OperatorEmployeeId` for CashierId |
| `CurrentUserDto` | `HasPin` moves to employee context; add operator info |

### API Layer

| Endpoint | Change |
|---|---|
| `POST /api/v1/roles` | NEW — Create role |
| `GET /api/v1/roles` | NEW — List roles |
| `PUT /api/v1/roles/{id}` | NEW — Update role + permissions |
| `DELETE /api/v1/roles/{id}` | NEW — Deactivate role |
| `POST /api/v1/auth/switch-operator` | NEW — PIN entry to switch active employee |
| `PATCH /api/v1/employees/{id}/pin` | NEW — Set employee PIN (admin only) |
| `PATCH /api/v1/auth/users/{id}/pin` | REMOVE — No longer on User |

### POS Frontend

| Area | Change |
|---|---|
| Auth flow | After Clerk login → PIN entry screen (not lock screen) |
| Lock screen | Merges with operator-switch (same PIN pad) |
| Navbar | Shows active employee name, "Switch" button instead of "Lock" |
| Shift/transaction pages | Operate under employee context, not Clerk user |
| Activity tracker | Auto-locks → returns to PIN entry (not unlock, but re-identify) |

### Admin Frontend

| Area | Change |
|---|---|
| Role management page (NEW) | CRUD for roles + permission checkbox grid |
| Employee detail page | Role dropdown + PIN management (if role has POS access) |
| Employee form | Add Role selector |
| Settings page | Keep lock timeout + max PIN attempts |

---

## Suggested Permissions

### POS Permissions

```
pos:open_shift        — Open/Close Shift
pos:add_to_queue      — Add to Queue
pos:create_transaction — Create Transaction
pos:apply_discount    — Apply Discount
pos:apply_tip         — Apply Tip
pos:void_transaction  — Void Transaction
pos:process_refund    — Process Refund
pos:cash_movement     — Record Cash In/Out
pos:view_shift_report — View Shift Report
```

### Admin Permissions

```
admin:manage_employees  — Create/Edit/Deactivate Employees
admin:manage_services   — Services, Pricing, Commissions
admin:manage_branches   — Branch CRUD + status
admin:manage_roles      — Role CRUD + permission assignment
admin:view_reports      — Dashboard, Reports, Analytics
admin:process_payroll   — Close/Process Payroll Periods
admin:manage_settings   — Tenant/Shift Settings
admin:review_shifts     — Approve/Flag Closed Shifts
```

---

## Migration Strategy

### If system has existing data:

1. Create `Role` table with default roles ("Owner", "Cashier")
2. Add `RoleId` + `PinHash` to Employee
3. Migrate: for each `CashierShift`/`Transaction` with `CashierId` pointing to `User.Id`, find the linked `Employee` and update to `Employee.Id`
4. Remove `PinHash` from User
5. Drop `Employee.UserId` FK (no longer needed — employees don't need Clerk accounts)

### If system is early enough (no production data):

- Clean swap: change FKs directly, drop old columns, add new ones

---

## Benefits

- **Cost savings** — fewer Clerk accounts (only admins/owners)
- **Simpler cashier onboarding** — no email/password, just Employee + Role + PIN
- **Faster operator switching** — PIN vs. full sign-out/sign-in
- **Granular permissions** — checkbox-based instead of Clerk's coarse `org_role`
- **Industry standard** — matches Loyverse, Square, Toast, Lightspeed

## Risks

- **Breaking change** — CashierShift/Transaction FK migration
- **Audit trail** — need to track both device owner (Clerk) and operator (Employee)
- **Security** — PIN-only is weaker than Clerk auth (acceptable for POS operator switching)
- **Admins on POS** — admins who also operate POS need both Clerk account AND Employee record
