# SplashSphere — Expense Approval Workflow

> **Phase:** 23.6 (Value-Add). Requires Phase 15 (Expense Tracking).
> **Plan gating:** Enterprise feature (multi-branch operations need approval chains).

---

## What It Does

Currently, any user can record an expense and it immediately hits the P&L. For multi-branch operations, the owner wants control: branch managers submit expenses, the owner reviews and approves/rejects before the expense is finalized. This prevents unauthorized spending and gives the owner visibility before it's too late.

---

## How It Works

```
Branch Manager submits expense:
  "Electricity bill — ₱3,500 — Makati branch"
       │
       ▼
  Status: PENDING APPROVAL
  Owner gets notification: "New expense ₱3,500 needs your approval"
       │
       ├──→ Owner APPROVES → expense status = APPROVED → hits P&L
       │
       └──→ Owner REJECTS (with reason) → expense status = REJECTED → does NOT hit P&L
                Manager notified: "Expense rejected: please attach the receipt"
```

---

## Domain Changes

Update existing `Expense` entity (from Phase 15):

```csharp
// Add to existing Expense entity:
public ExpenseApprovalStatus ApprovalStatus { get; set; } = ExpenseApprovalStatus.AutoApproved;
public string? ApprovedById { get; set; }
public DateTime? ApprovedAt { get; set; }
public string? RejectionReason { get; set; }
public bool RequiresApproval { get; set; }          // Based on tenant setting

public enum ExpenseApprovalStatus
{
    AutoApproved,       // No approval workflow (default for Starter/Growth)
    PendingApproval,    // Submitted, waiting for owner
    Approved,           // Owner approved
    Rejected            // Owner rejected
}
```

---

## Tenant Settings

```
/settings → Expense Management:
  Require approval for expenses:  [✓]
  Auto-approve expenses under:    ₱[500] (small expenses skip approval)
  Approver:                       [Owner only ▾] / [Owner + Managers]
```

**Rules:**
- If approval is disabled: all expenses auto-approve (current behavior)
- If enabled: expenses above the threshold go to PENDING, below auto-approve
- Owner (admin role) always sees all pending expenses
- Manager role submits but can also approve if setting allows

---

## Admin UI

### Pending Approvals Widget (Dashboard)

```
┌── Pending Approvals (3) ──────────────────────────────┐
│  ₱3,500  Electricity — Makati    [✓ Approve] [✗ Reject]│
│  ₱1,200  Cleaning supplies       [✓ Approve] [✗ Reject]│
│  ₱8,500  Pressure washer repair   [✓ Approve] [✗ Reject]│
└────────────────────────────────────────────────────────┘
```

### Expenses Page Enhancement

Add filter tab: `[All] [Pending] [Approved] [Rejected]`

Pending expenses show approve/reject buttons inline. Reject opens a dialog for the reason.

---

## API Endpoints

| Method | Route | Description |
|---|---|---|
| `PATCH` | `/expenses/{id}/approve` | Approve an expense |
| `PATCH` | `/expenses/{id}/reject` | Reject with reason |
| `GET` | `/expenses/pending` | List pending approvals for current user |

---

## P&L Impact

**Only APPROVED and AUTO_APPROVED expenses appear in the P&L report.** PENDING and REJECTED expenses are excluded. This ensures the P&L reflects actual authorized spending.

---

## Claude Code Prompt

```
Add expense approval workflow:

Update Expense entity: add ApprovalStatus, ApprovedById, ApprovedAt, RejectionReason.
Update CreateExpenseCommandHandler: check tenant setting — if approval required
  AND amount > threshold, set status = PendingApproval instead of AutoApproved.
  Send notification to owner.

New commands: ApproveExpenseCommand, RejectExpenseCommand
Update GetProfitLossQuery: only include Approved + AutoApproved expenses.

Admin: pending approvals widget on dashboard, filter tabs on expenses page,
  approve/reject buttons with rejection reason dialog.
Notifications: expense.pending_approval, expense.approved, expense.rejected.

Tenant settings: /settings toggle for approval requirement + auto-approve threshold.
Plan gating: Enterprise feature.
```
