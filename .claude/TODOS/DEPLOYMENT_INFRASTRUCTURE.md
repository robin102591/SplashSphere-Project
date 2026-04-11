# SplashSphere — Deployment & Infrastructure

> **Target:** Containerized deployment. Docker Compose for dev, managed platform for production.
> **Hosting:** Railway/Render for API, Vercel for Next.js apps, Neon for PostgreSQL.

---

## Service Map

| Service | Tech | Port | Hosting |
|---|---|---|---|
| API | .NET 9 (+ Hangfire + SignalR) | 5000 | Railway / Render |
| Admin App | Next.js 16 | 3000 | Vercel |
| POS App | Next.js 16 (PWA) | 3001 | Vercel |
| Super Admin | Blazor Server | 5001 | Railway |
| PostgreSQL | PostgreSQL 16 | 5432 | Neon / Supabase |
| Redis | Redis 7 | 6379 | Upstash |
| DNS + CDN | Cloudflare | — | Cloudflare |

**Subdomains:** `app.splashsphere.ph` (admin), `pos.splashsphere.ph` (POS), `api.splashsphere.ph` (API), `ops.splashsphere.ph` (super admin)

---

## Docker

### API Dockerfile (multi-stage, Alpine, non-root)
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src
COPY src/ .
RUN dotnet publish API/SplashSphere.API.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine
WORKDIR /app
COPY --from=build /app .
RUN addgroup -S app && adduser -S app -G app
USER app
EXPOSE 5000
HEALTHCHECK CMD wget -qO- http://localhost:5000/health || exit 1
ENTRYPOINT ["dotnet", "SplashSphere.API.dll"]
```

### Frontend Dockerfile (Next.js standalone output)
```dockerfile
FROM node:22-alpine AS builder
WORKDIR /app
COPY pnpm-lock.yaml pnpm-workspace.yaml package.json ./
COPY apps/admin/ apps/admin/
COPY packages/types/ packages/types/
RUN corepack enable && pnpm install --frozen-lockfile && pnpm --filter admin build

FROM node:22-alpine
WORKDIR /app
COPY --from=builder /app/apps/admin/.next/standalone ./
COPY --from=builder /app/apps/admin/.next/static ./.next/static
COPY --from=builder /app/apps/admin/public ./public
RUN addgroup -S app && adduser -S app -G app
USER app
EXPOSE 3000
CMD ["node", "server.js"]
```

### docker-compose.yml (Development)
```yaml
services:
  api:
    build: { context: ., dockerfile: src/SplashSphere.API/Dockerfile }
    ports: ["5000:5000"]
    env_file: .env
    depends_on: [db, redis]
  admin:
    build: { context: ., dockerfile: apps/admin/Dockerfile }
    ports: ["3000:3000"]
    env_file: .env
  pos:
    build: { context: ., dockerfile: apps/pos/Dockerfile }
    ports: ["3001:3001"]
    env_file: .env
  db:
    image: postgres:16-alpine
    ports: ["5432:5432"]
    environment: { POSTGRES_DB: splashsphere, POSTGRES_USER: postgres, POSTGRES_PASSWORD: postgres }
    volumes: [pgdata:/var/lib/postgresql/data]
  redis:
    image: redis:7-alpine
    ports: ["6379:6379"]
volumes:
  pgdata:
```

---

## Environment Variables

### API
| Variable | Description |
|---|---|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection (with SSL in prod) |
| `ConnectionStrings__SplashSphereReadOnly` | Read replica for super admin |
| `Redis__ConnectionString` | Redis URL |
| `Clerk__SecretKey` / `Clerk__PublishableKey` | Clerk auth |
| `PayMongo__SecretKey` / `PayMongo__WebhookSecret` | Payment gateway |
| `Semaphore__ApiKey` / `Semaphore__SenderName` | SMS |
| `Resend__ApiKey` | Email |
| `Anthropic__ApiKey` / `Anthropic__Model` | AI |
| `Hangfire__DashboardPassword` | Hangfire UI auth |

### Frontend
| Variable | Description |
|---|---|
| `NEXT_PUBLIC_API_URL` | API base URL |
| `NEXT_PUBLIC_CLERK_PUBLISHABLE_KEY` | Clerk frontend key |

---

## CI/CD (GitHub Actions)

```yaml
# .github/workflows/deploy.yml
on: { push: { branches: [main] } }
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - run: dotnet test --configuration Release
      - run: pnpm install && pnpm --filter admin test && pnpm --filter pos test

  deploy:
    needs: test
    steps:
      - uses: docker/build-push-action@v5
        with: { push: true, tags: "ghcr.io/lezanobtech/splashsphere-api:${{ github.sha }}" }
      # Trigger Railway/Render redeploy via webhook
```

---

## Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgresql")
    .AddRedis(redisConnection, name: "redis");

app.MapHealthChecks("/health");
```

---

## Database Migrations (Production)

Never auto-migrate on startup. Run as a separate step:
```bash
dotnet ef database update --project Infrastructure --startup-project API
```
Always snapshot before migrating production.

---

## Estimated Monthly Cost (Startup)

| Component | Service | Cost |
|---|---|---|
| PostgreSQL | Neon Pro | $19 |
| API | Railway | $10-20 |
| Admin App | Vercel Free/Pro | $0-20 |
| POS App | Vercel Free/Pro | $0-20 |
| Super Admin | Railway | $7 |
| Redis | Upstash | $0-10 |
| DNS + CDN | Cloudflare | $0 |
| **Total** | | **$36-97/mo** |

Graduate to DigitalOcean App Platform or AWS ECS at 100+ tenants.

---

## Claude Code Prompt

```
Set up deployment:
1. Create Dockerfiles (API, admin, pos) — multi-stage, Alpine, non-root
2. Create docker-compose.yml for local dev
3. Add health check endpoint (/health)
4. Create .github/workflows/test.yml and deploy.yml
5. Create .env.example with all variables
6. Add .dockerignore files
```
