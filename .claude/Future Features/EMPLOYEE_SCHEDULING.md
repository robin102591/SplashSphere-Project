# SplashSphere — Employee Scheduling / Shift Roster

> **Phase:** 23.5 (Value-Add). Requires employee + attendance system (core).
> **Plan gating:** Growth+ feature.

---

## What It Does

The manager plans the weekly schedule ahead of time — who works which days, what shift (morning/afternoon/full day). Employees see their upcoming schedule on the self-service portal. The system auto-populates attendance records from the schedule and flags discrepancies (scheduled but didn't show, not scheduled but clocked in).

---

## Domain Models

```csharp
public sealed class ShiftSchedule
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public DateTime WeekStartDate { get; set; }                 // Monday of the week
    public ScheduleStatus Status { get; set; } = ScheduleStatus.Draft;
    public string? PublishedById { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<ScheduleEntry> Entries { get; set; } = [];
}

public sealed class ScheduleEntry
{
    public string Id { get; set; } = string.Empty;
    public string ScheduleId { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public ShiftType Shift { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public bool IsDayOff { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum ScheduleStatus { Draft, Published }
public enum ShiftType { Morning, Afternoon, FullDay, Custom, DayOff }
```

---

## Admin UI — Weekly Schedule (`/schedule`)

```
┌── Week of March 25-30, 2026 — Makati ─── [Publish] ──┐
│                                                        │
│  [← Prev Week]  [This Week]  [Next Week →]           │
│  Status: Draft                                         │
│                                                        │
│              Mon   Tue   Wed   Thu   Fri   Sat         │
│  Juan D.     AM    AM    —     Full  AM    Full        │
│  Pedro S.    Full  Full  AM    —     Full  Full        │
│  Maria G.    PM    PM    Full  Full  —     AM          │
│  Ana R.      Full  Full  Full  Full  Full  —           │
│  Carlos M.   AM    —     PM    PM    AM    Full        │
│                                                        │
│  AM = 6:00-12:00 | PM = 12:00-18:00 | Full = 6:00-18:00│
│  — = Day Off                                           │
│                                                        │
│  Click any cell to change. Drag to copy.              │
│  [Copy from Last Week]  [Auto-Fill]                    │
│                                                        │
└────────────────────────────────────────────────────────┘
```

**Publish:** When the manager publishes the schedule, employees see it on their portal and optionally receive an SMS: "Your schedule for Mar 25-30 is posted. Check the app."

**Copy from Last Week:** One-click duplicate the previous week's schedule. Most car washes have a repeating pattern.

**Auto-Fill:** Distribute employees evenly across days, ensuring minimum staffing per day (configurable).

---

## Employee Portal Integration

Add a "My Schedule" tab to the Employee Self-Service Portal:

```
┌── My Schedule — Week of March 25 ─────────────────────┐
│                                                        │
│  Mon Mar 25   Morning (6:00 AM - 12:00 PM)            │
│  Tue Mar 26   Morning (6:00 AM - 12:00 PM)            │
│  Wed Mar 27   Day Off                                  │
│  Thu Mar 28   Full Day (6:00 AM - 6:00 PM)            │
│  Fri Mar 29   Morning (6:00 AM - 12:00 PM)            │
│  Sat Mar 30   Full Day (6:00 AM - 6:00 PM)            │
│                                                        │
│  Next week: Not yet published                          │
│                                                        │
└────────────────────────────────────────────────────────┘
```

---

## Attendance Auto-Population

When a schedule is published, the system pre-creates `Attendance` records for each scheduled employee/date with `isPresent = false`. When the employee clocks in, the existing record is updated. This enables discrepancy tracking:

| Scenario | What Happens |
|---|---|
| Scheduled + clocked in | Normal — `isPresent = true` |
| Scheduled + didn't clock in | Flag: "No-show" (scheduled but absent) |
| Not scheduled + clocked in | Flag: "Unscheduled" (extra employee) |
| Day off + didn't clock in | Normal — no flag |

---

## API Endpoints

| Method | Route | Description |
|---|---|---|
| `GET` | `/schedules` | List weekly schedules (filter by branch, week) |
| `POST` | `/schedules` | Create a new weekly schedule |
| `PUT` | `/schedules/{id}` | Update schedule entries (only in Draft) |
| `PATCH` | `/schedules/{id}/publish` | Publish schedule (locks editing, notifies employees) |
| `POST` | `/schedules/{id}/copy-from/{sourceId}` | Copy entries from another week |
| `GET` | `/employee-portal/schedule` | Get my schedule (employee view) |

---

## Claude Code Prompt

```
Build Employee Scheduling:

Domain: ShiftSchedule, ScheduleEntry, ScheduleStatus, ShiftType enums
Application: CreateScheduleCommand, UpdateScheduleCommand, PublishScheduleCommand,
  CopyFromWeekCommand, GetScheduleQuery, GetMyScheduleQuery (employee portal)

On publish: pre-create Attendance records for scheduled employees.
Discrepancy tracking: flag no-shows and unscheduled clock-ins.

Admin: /schedule page with weekly grid view. Click cell to change shift type.
  Copy from last week button. Publish with SMS notification to employees.
Employee portal: "My Schedule" tab showing upcoming week.

Plan gating: Growth+ feature.
```
