---
name: splashsphere-context
description: Core project context shared by all agents
---

# SplashSphere — Shared Context

## Project Identity
- **Company:** LezanobTech
- **Product:** SplashSphere — multi-tenant car wash management SaaS
- **Market:** Philippine car wash businesses
- **Domain:** Manual car wash operations, commission-based employees, weekly payroll

## Tech Stack
- Backend: .NET 9, Clean Architecture, CQRS/MediatR, EF Core 9, PostgreSQL
- Auth: Clerk (JWT with org_id for tenant isolation)
- Background jobs: Hangfire
- Real-time: SignalR
- Frontend: Two Next.js 16 apps (admin + POS) in a pnpm monorepo
- Shared types: packages/types/ TypeScript package
- Payments: PayMongo (Philippine gateway)

## Solution Structure
```
SplashSphere/
├── src/
│   ├── SplashSphere.Domain/          # Entities, enums, value objects, events
│   ├── SplashSphere.Application/     # CQRS handlers, interfaces, DTOs
│   ├── SplashSphere.Infrastructure/  # EF Core, external services, auth
│   └── SplashSphere.API/             # Minimal API endpoints, middleware
├── apps/
│   ├── admin/                        # Next.js 16 — admin dashboard
│   └── pos/                          # Next.js 16 — POS terminal
├── packages/
│   └── types/                        # Shared TypeScript types
└── tests/
```

## Philippine Business Context
- Services are manual (attendants, not machines)
- Employees: COMMISSION type (paid per service) or DAILY type (fixed daily rate) or HYBRID (both)
- Payroll: weekly or semi-monthly cutoff, configurable per branch
- Payments: Cash, GCash, Maya, credit/debit card, bank transfer
- Pricing: varies by vehicle type × vehicle size (matrix)
- Commission: also varies by vehicle type × size (separate matrix)
- Currency: Philippine Peso (₱), formatted with Intl.NumberFormat('en-PH')
