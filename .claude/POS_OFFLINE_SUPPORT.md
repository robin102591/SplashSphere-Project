# SplashSphere — POS Offline Support

> **Phase:** 19 (can be built independently after core POS is functional).
> **Scope:** POS app only. Admin dashboard does NOT need offline support.
> **Plan gating:** Available on all plans. Offline is an operational necessity, not a premium feature.

---

## Why This Matters

Philippine internet is unreliable. A car wash in Quezon City might lose connectivity for 15 minutes during a thunderstorm. A branch in a provincial area might have intermittent signal all day. When the internet drops, the cashier has two choices: stop working and lose customers, or write transactions on paper and enter them later (error-prone, commissions get lost).

SplashSphere's POS must keep working when the internet dies. The cashier should barely notice the difference — transactions process instantly, the queue updates, and when connectivity returns, everything syncs silently in the background.

**The core principle: IndexedDB is the source of truth during offline. The server is synced to eventually.**

---

## Architecture

```
┌───────────────────────────────────────────────────────────────┐
│  POS APP (Next.js PWA)                                        │
│                                                               │
│  ┌──────────────┐    ┌──────────────┐    ┌────────────────┐   │
│  │ React UI     │───▶│ Offline Store │───▶│ Sync Engine    │   │
│  │ (components) │    │ (IndexedDB)   │    │ (background)   │   │
│  └──────────────┘    └──────────────┘    └───────┬────────┘   │
│         │                    ▲                    │            │
│         │                    │              ┌─────▼─────┐     │
│         ▼                    │              │ Sync Queue │     │
│  ┌──────────────┐            │              │ (IndexedDB)│     │
│  │ Service      │            │              └─────┬─────┘     │
│  │ Worker       │            │                    │            │
│  │ (Serwist)    │            │              ┌─────▼─────┐     │
│  └──────────────┘            │              │ .NET API   │     │
│                              │              │ (server)   │     │
│                              │              └─────┬─────┘     │
│                              │                    │            │
│                              └────────────────────┘            │
│                              (reconcile on sync success)       │
└───────────────────────────────────────────────────────────────┘
```

**Three layers:**

1. **Service Worker (Serwist)** — Caches the app shell, static assets, and fonts. Makes the POS installable as a PWA. Handles navigation requests when offline.

2. **Offline Store (IndexedDB via `idb`)** — Local database that mirrors critical server data: services, pricing matrices, employees, vehicles, customers, active shift. The UI reads from here first, not the API.

3. **Sync Engine** — Queues mutations (transactions, cash movements, shift operations) when offline. Replays them to the server when connectivity returns. Handles conflicts and retries.

---

## What Works Offline

### Fully Functional Offline

| Feature | How |
|---|---|
| **Create transaction** | Generated locally with temp ID, saved to IndexedDB, queued for sync |
| **Look up vehicle by plate** | Cached in IndexedDB (synced periodically when online) |
| **Select services** | Service list + pricing matrix cached in IndexedDB |
| **Assign employees** | Employee list cached in IndexedDB (filtered to clocked-in) |
| **Accept cash payment** | Recorded locally, no server needed |
| **Print receipt** | Receipt generated from local data |
| **Record cash movement** | Saved to IndexedDB, queued for sync |
| **View shift summary** | Calculated from local shift data |
| **View transaction history** | Local transactions available, server history cached |

### Degraded Offline (Works with Limitations)

| Feature | Limitation |
|---|---|
| **GCash / Card payment** | Payment recorded locally, but actual payment gateway verification requires connectivity. Mark as "pending verification" and verify on sync. |
| **Queue board** | Local queue data visible, but no real-time updates from other POS terminals. Other terminals won't see this terminal's changes until sync. |
| **SMS notifications** | Queued, sent when online |
| **Customer loyalty points** | Earned locally, synced later. Redemption blocked offline (prevents double-spend). |

### Not Available Offline

| Feature | Reason |
|---|---|
| **Sign in / sign out** | Clerk auth requires server. Solution: keep session alive with long-lived token. |
| **Admin dashboard** | Different app entirely, not offline-enabled |
| **Real-time SignalR events** | Requires server connection. Reconnects automatically. |
| **Plan enforcement checks** | Cached plan data used. Re-verified on sync. |

---

## IndexedDB Schema

Using the `idb` library for a clean async API over IndexedDB.

```typescript
// lib/offline/db.ts
import { openDB, DBSchema } from 'idb';

interface SplashSpherePOSDB extends DBSchema {
  // Reference data (cached from server)
  services: {
    key: string;          // serviceId
    value: CachedService; // includes pricing matrix
    indexes: { 'by-category': string };
  };
  servicePackages: {
    key: string;
    value: CachedServicePackage;
  };
  employees: {
    key: string;
    value: CachedEmployee;
    indexes: { 'by-branch': string; 'by-type': string };
  };
  vehicles: {
    key: string;          // carId
    value: CachedVehicle;
    indexes: { 'by-plate': string; 'by-customer': string };
  };
  customers: {
    key: string;
    value: CachedCustomer;
    indexes: { 'by-name': string; 'by-contact': string };
  };
  sizes: {
    key: string;
    value: CachedSize;
  };
  vehicleTypes: {
    key: string;
    value: CachedVehicleType;
  };
  merchandise: {
    key: string;
    value: CachedMerchandise;
  };
  tenantPlan: {
    key: string;          // 'current'
    value: CachedTenantPlan;
  };

  // Operational data (created/modified locally)
  localTransactions: {
    key: string;          // local temp ID (ulid)
    value: LocalTransaction;
    indexes: { 'by-sync-status': SyncStatus; 'by-date': string };
  };
  localCashMovements: {
    key: string;
    value: LocalCashMovement;
    indexes: { 'by-sync-status': SyncStatus };
  };
  activeShift: {
    key: string;          // 'current'
    value: LocalShift;
  };

  // Sync management
  syncQueue: {
    key: string;          // queueItemId
    value: SyncQueueItem;
    indexes: { 'by-status': SyncStatus; 'by-created': string };
  };

  // Metadata
  syncMeta: {
    key: string;          // 'lastSync', 'cacheVersion', etc.
    value: { key: string; value: string; updatedAt: string };
  };
}

export type SyncStatus = 'pending' | 'syncing' | 'synced' | 'failed' | 'conflict';

export interface SyncQueueItem {
  id: string;
  operation: 'create_transaction' | 'update_transaction' | 'create_cash_movement'
    | 'open_shift' | 'close_shift' | 'record_attendance';
  payload: any;
  tempId: string;           // Local ID before server assigns real ID
  serverId?: string;        // Server ID after sync (for ID mapping)
  status: SyncStatus;
  retryCount: number;
  maxRetries: number;       // Default: 5
  errorMessage?: string;
  createdAt: string;
  lastAttemptAt?: string;
}

export interface LocalTransaction {
  tempId: string;           // ULID generated client-side
  serverId?: string;        // Assigned after sync
  syncStatus: SyncStatus;
  // Full transaction data matching server DTO
  tenantId: string;
  branchId: string;
  customerId?: string;
  cashierId: string;
  transactionNo: string;    // Generated locally: {BranchCode}-{Date}-{LocalSeq}
  services: LocalTransactionService[];
  packages: LocalTransactionPackage[];
  merchandise: LocalTransactionMerchandise[];
  employees: LocalTransactionEmployee[];
  payments: LocalPayment[];
  totalAmount: number;
  discountAmount: number;
  finalAmount: number;
  totalCommissionAmount: number;
  status: string;
  transactionDate: string;
  createdAt: string;
}
```

---

## Data Caching Strategy

### What Gets Cached & When

| Data | Cache Strategy | TTL | Size Estimate |
|---|---|---|---|
| Services + pricing matrices | Eager: cache on app load, refresh every 30 min | 30 min | ~50KB |
| Service packages | Eager: same as services | 30 min | ~20KB |
| Employees (branch) | Eager: cache on shift open | 30 min | ~10KB |
| Vehicles (recent 500) | Lazy: cache on lookup, keep last 500 | 24 hours | ~100KB |
| Customers (recent 500) | Lazy: cache on lookup, keep last 500 | 24 hours | ~50KB |
| Sizes + Vehicle Types | Eager: rarely changes | 24 hours | ~2KB |
| Merchandise | Eager: cache on app load | 30 min | ~20KB |
| Tenant plan | Eager: cache on sign-in | 1 hour | ~1KB |
| Active shift | Local: created on shift open | Session | ~5KB |
| Local transactions | Local: created on transaction | Until synced + 7 days | Variable |
| Sync queue | Local: mutation queue | Until synced | Variable |

**Total estimated cache: < 500KB** for a typical branch. Well within IndexedDB limits.

### Cache Refresh Flow

```
App opens (or every 30 minutes while online):
  1. Check navigator.onLine
  2. If online:
     a. Fetch /api/v1/services?branchId={branch} → store in IndexedDB "services"
     b. Fetch /api/v1/employees?branchId={branch}&isActive=true → store in "employees"
     c. Fetch /api/v1/merchandise → store in "merchandise"
     d. Fetch /api/v1/sizes + /api/v1/vehicle-types → store
     e. Fetch /api/v1/billing/plan → store in "tenantPlan"
     f. Update syncMeta.lastSync = now
  3. If offline:
     a. Read from existing IndexedDB cache
     b. Show age indicator: "Data last synced 15 min ago"
```

### Vehicle/Customer Lookup Cache

```
Cashier types plate number "ABC-1234":
  1. Check IndexedDB "vehicles" by plate index
  2. If found and age < 24 hours → return from cache (instant)
  3. If not found or stale:
     a. If online → fetch from API, cache result, return
     b. If offline → return cached version (even if stale) or "Vehicle not found locally"
  4. New vehicles created during transaction → cached immediately
```

---

## Offline Transaction Flow

This is the most critical flow — the cashier must be able to complete a transaction regardless of connectivity.

### Online Flow (Normal)

```
Cashier → API → Database → Response → UI updates
```

### Offline Flow

```
Cashier → IndexedDB (immediate) → UI updates → Sync Queue → (later) API → Reconcile
```

### Step-by-Step Offline Transaction

```
1. PLATE LOOKUP
   - Search IndexedDB "vehicles" by plate
   - If found: load vehicle + customer data from cache
   - If not found: allow manual entry (vehicle type, size selection)
   - New vehicle saved to IndexedDB immediately

2. SERVICE SELECTION
   - Read services from IndexedDB (cached with full pricing matrix)
   - Resolve price: service × vehicleType × size from cached matrix
   - If no matrix match: use basePrice (show "base price" label)

3. EMPLOYEE ASSIGNMENT
   - Read employees from IndexedDB (filtered to COMMISSION type, active)
   - Calculate commission split from cached commission matrix
   - If no matrix match: use default calculation

4. PAYMENT
   - Cash: processed fully offline. Change calculated locally.
   - GCash/Card: recorded with status "pending_verification".
     Amount captured, but actual gateway call deferred to sync.
     Show cashier: "GCash payment will be verified when online"
   
5. COMPLETE TRANSACTION
   a. Generate local transaction ID: ULID (universally unique, sortable)
   b. Generate local transaction number: {BranchCode}-{Date}-{LocalSeq}
      LocalSeq increments per branch per day, stored in IndexedDB
   c. Build full transaction object with all line items, commissions, payments
   d. Save to IndexedDB "localTransactions" with syncStatus = 'pending'
   e. Add to IndexedDB "syncQueue": operation = 'create_transaction'
   f. Update local supply stock (if ServiceSupplyUsage is configured)
   g. Update local merchandise stock (if merchandise items sold)
   h. Show receipt (generated from local data)
   i. UI shows green checkmark: "Transaction saved"
      With subtle indicator: "⏳ Waiting to sync" (only if offline)

6. SYNC (when online)
   a. Sync engine picks up queued item
   b. POST /api/v1/transactions with full transaction data
   c. Server processes normally, returns server-assigned ID + transactionNo
   d. Update IndexedDB: localTransaction.serverId = serverResponse.id
   e. Update IndexedDB: localTransaction.syncStatus = 'synced'
   f. Remove from sync queue
   g. UI indicator changes: "⏳" disappears silently
```

---

## Sync Engine

### Core Sync Loop

```typescript
// lib/offline/sync-engine.ts

class SyncEngine {
  private isRunning = false;
  private intervalId: number | null = null;

  start() {
    // Process queue every 10 seconds when online
    this.intervalId = setInterval(() => this.processQueue(), 10_000);
    // Also trigger immediately when connectivity returns
    window.addEventListener('online', () => this.processQueue());
  }

  async processQueue() {
    if (!navigator.onLine || this.isRunning) return;
    this.isRunning = true;

    try {
      const db = await getDB();
      const pendingItems = await db.getAllFromIndex('syncQueue', 'by-status', 'pending');

      for (const item of pendingItems) {
        if (item.retryCount >= item.maxRetries) {
          item.status = 'failed';
          await db.put('syncQueue', item);
          continue;
        }

        item.status = 'syncing';
        item.lastAttemptAt = new Date().toISOString();
        await db.put('syncQueue', item);

        try {
          const result = await this.executeOperation(item);
          
          // Success: update local records with server IDs
          await this.reconcile(item, result);
          item.status = 'synced';
          item.serverId = result.id;
          await db.put('syncQueue', item);

        } catch (error) {
          item.status = 'pending';
          item.retryCount++;
          item.errorMessage = error.message;
          await db.put('syncQueue', item);

          // If it's a conflict (409), mark for manual resolution
          if (error.status === 409) {
            item.status = 'conflict';
            await db.put('syncQueue', item);
          }
          
          // Stop processing on network error (will retry next cycle)
          if (!navigator.onLine) break;
        }
      }
    } finally {
      this.isRunning = false;
    }
  }

  private async executeOperation(item: SyncQueueItem): Promise<any> {
    switch (item.operation) {
      case 'create_transaction':
        return apiClient('/api/v1/transactions/offline-sync', {
          method: 'POST',
          body: JSON.stringify({
            ...item.payload,
            offlineTempId: item.tempId,
            offlineCreatedAt: item.createdAt,
          }),
        });
      case 'create_cash_movement':
        return apiClient('/api/v1/shifts/current/cash-movement', {
          method: 'POST',
          body: JSON.stringify(item.payload),
        });
      case 'open_shift':
        return apiClient('/api/v1/shifts/open', {
          method: 'POST',
          body: JSON.stringify(item.payload),
        });
      // ... other operations
    }
  }

  private async reconcile(item: SyncQueueItem, serverResult: any) {
    const db = await getDB();
    
    if (item.operation === 'create_transaction') {
      // Update local transaction with server ID
      const localTx = await db.get('localTransactions', item.tempId);
      if (localTx) {
        localTx.serverId = serverResult.id;
        localTx.transactionNo = serverResult.transactionNo; // Server may assign different number
        localTx.syncStatus = 'synced';
        await db.put('localTransactions', localTx);
      }
    }
  }
}
```

### Server-Side Offline Sync Endpoint

The server needs a special endpoint that handles transactions created offline:

```csharp
// POST /api/v1/transactions/offline-sync
// Differs from normal CreateTransaction:
// 1. Accepts offlineTempId for idempotency (prevents duplicate creation on retry)
// 2. Accepts offlineCreatedAt to preserve the original transaction time
// 3. Returns the server-assigned ID and transactionNo

[HttpPost("offline-sync")]
public async Task<IResult> SyncOfflineTransaction(
    OfflineSyncTransactionCommand command, ISender sender)
{
    // Check if this tempId was already synced (idempotency)
    var existing = await _db.Transactions
        .FirstOrDefaultAsync(t => t.OfflineTempId == command.OfflineTempId);
    
    if (existing != null)
        return Results.Ok(new { id = existing.Id, transactionNo = existing.TransactionNo });
    
    // Process normally but use offlineCreatedAt as transactionDate
    var result = await sender.Send(command);
    return Results.Ok(result);
}
```

Add to the `Transaction` entity:
```csharp
public string? OfflineTempId { get; set; }  // For idempotent offline sync
```

---

## Connectivity Detection

```typescript
// lib/offline/connectivity.ts

export function useConnectivity() {
  const [status, setStatus] = useState<'online' | 'offline' | 'reconnecting'>(() =>
    typeof navigator !== 'undefined' ? (navigator.onLine ? 'online' : 'offline') : 'online'
  );
  const [pendingSyncCount, setPendingSyncCount] = useState(0);

  useEffect(() => {
    const handleOnline = () => setStatus('online');
    const handleOffline = () => setStatus('offline');
    
    window.addEventListener('online', handleOnline);
    window.addEventListener('offline', handleOffline);
    
    // Also ping the API periodically (navigator.onLine can be wrong)
    const pingInterval = setInterval(async () => {
      try {
        await fetch('/api/v1/health', { method: 'HEAD', cache: 'no-store' });
        setStatus('online');
      } catch {
        setStatus('offline');
      }
    }, 30_000); // Every 30 seconds

    return () => {
      window.removeEventListener('online', handleOnline);
      window.removeEventListener('offline', handleOffline);
      clearInterval(pingInterval);
    };
  }, []);

  // Count pending sync items
  useEffect(() => {
    const checkPending = async () => {
      const db = await getDB();
      const pending = await db.getAllFromIndex('syncQueue', 'by-status', 'pending');
      setPendingSyncCount(pending.length);
    };
    checkPending();
    const interval = setInterval(checkPending, 5_000);
    return () => clearInterval(interval);
  }, []);

  return { status, pendingSyncCount };
}
```

### Connection Status UI

In the POS top bar:

```
Online:       🟢 (green dot, no text — don't clutter)
Offline:      🔴 "Offline — transactions saved locally" (amber banner below top bar)
Reconnecting: 🟡 "Reconnecting..." (amber dot, pulsing)
Syncing:      🟢 "Syncing 3 items..." (green dot with count, fades when done)
Sync Failed:  🔴 "2 items failed to sync" (red, tappable → shows details)
```

---

## Service Worker & PWA Setup

### Install Serwist

```bash
pnpm add serwist @serwist/next
```

### Service Worker Configuration

```typescript
// apps/pos/src/sw.ts
import { defaultCache } from '@serwist/next/worker';
import { Serwist } from 'serwist';

const serwist = new Serwist({
  precacheEntries: self.__SW_MANIFEST,  // Auto-generated precache manifest
  skipWaiting: true,
  clientsClaim: true,
  navigationPreload: true,
  runtimeCaching: [
    ...defaultCache,
    // Cache API GET responses (stale-while-revalidate)
    {
      urlPattern: /\/api\/v1\/(services|employees|sizes|vehicle-types|merchandise)/,
      handler: 'StaleWhileRevalidate',
      options: {
        cacheName: 'api-reference-data',
        expiration: { maxAgeSeconds: 30 * 60 }, // 30 minutes
      },
    },
    // Cache fonts
    {
      urlPattern: /fonts\.googleapis\.com/,
      handler: 'CacheFirst',
      options: {
        cacheName: 'google-fonts',
        expiration: { maxAgeSeconds: 60 * 60 * 24 * 365 }, // 1 year
      },
    },
  ],
  // Offline fallback page
  fallbacks: {
    entries: [{ url: '/offline', matcher: ({ request }) => request.destination === 'document' }],
  },
});

serwist.addEventListeners();
```

### PWA Manifest

```json
// apps/pos/public/manifest.json
{
  "name": "SplashSphere POS",
  "short_name": "SplashSphere",
  "description": "Car wash POS terminal",
  "start_url": "/",
  "display": "standalone",
  "orientation": "any",
  "background_color": "#0f172a",
  "theme_color": "#0ea5e9",
  "icons": [
    { "src": "/icons/icon-192.png", "sizes": "192x192", "type": "image/png" },
    { "src": "/icons/icon-512.png", "sizes": "512x512", "type": "image/png" },
    { "src": "/icons/icon-maskable.png", "sizes": "512x512", "type": "image/png", "purpose": "maskable" }
  ]
}
```

### Offline Fallback Page

```
/offline — shown when navigating to an uncached page while offline:
"You're currently offline. The POS transaction screen is still available."
[Go to POS] button → navigates to cached POS home page
```

---

## Conflict Resolution

### Types of Conflicts

| Conflict | Scenario | Resolution |
|---|---|---|
| **Duplicate transaction** | Same offline transaction synced twice (network glitch) | Idempotency via `OfflineTempId` — server detects duplicate and returns existing record |
| **Transaction number collision** | Two offline terminals generate the same transaction number | Server generates the authoritative number on sync. Local number is a placeholder. |
| **Stale pricing** | Service price changed on server while POS was offline | Transaction synced with the price at time of sale (captured in the local transaction). Server logs a `PriceDiscrepancyEvent` for the admin to review. |
| **Employee not found** | Employee was deactivated while POS was offline | Server rejects the sync with 409 Conflict. Sync engine marks as `conflict`. Cashier resolves manually. |
| **Stale stock** | Merchandise sold offline but stock already at zero on server | Server allows negative stock (with warning flag). Admin reviews. |
| **Shift already closed** | Cash movement recorded after shift was closed on another terminal | Server rejects. Sync engine marks as `conflict`. Cashier resolves by assigning to correct shift. |

### Conflict UI

When a sync conflict occurs, show a non-blocking notification:

```
🔴 "1 transaction needs attention — prices may have changed since it was created offline"
[Review] → opens a conflict resolution dialog showing:
  - What was submitted (offline data)
  - What the server expected
  - Options: "Keep offline version" / "Accept server version" / "Ask manager"
```

Most conflicts should be rare and auto-resolvable. The common case (duplicate transaction) is handled silently via idempotency.

---

## Session & Auth Handling

Clerk tokens expire after 60 seconds by default. Offline, the POS can't refresh tokens.

### Strategy

```
1. On sign-in: cache the Clerk session token AND the decoded user/tenant context
   (userId, tenantId, branchId, role) in IndexedDB.

2. While online: Clerk refreshes tokens normally (every 50 seconds).
   Update cached session data on each refresh.

3. When offline: 
   - Clerk refresh fails silently (expected).
   - POS continues using cached user context for local operations.
   - API calls are queued, not executed (no valid token needed).
   
4. When online again:
   - Clerk re-authenticates automatically.
   - Fresh token used for sync queue replay.
   - If session expired entirely (long offline), redirect to sign-in.

5. Maximum offline session: 8 hours.
   After 8 hours offline, POS shows: "Please reconnect to continue. 
   Your offline transactions are saved and will sync when online."
   This prevents indefinite offline operation without auth verification.
```

---

## Data Hooks (React)

```typescript
// hooks/use-offline-services.ts
// Reads services from IndexedDB first, refreshes from API when online

export function useOfflineServices() {
  return useQuery({
    queryKey: ['services', 'offline'],
    queryFn: async () => {
      const db = await getDB();
      const cached = await db.getAll('services');
      
      if (navigator.onLine) {
        try {
          const fresh = await apiClient<Service[]>('/api/v1/services');
          // Update cache
          const tx = db.transaction('services', 'readwrite');
          await Promise.all(fresh.map(s => tx.store.put(s)));
          await tx.done;
          return fresh;
        } catch {
          return cached; // API failed, use cache
        }
      }
      
      return cached; // Offline, use cache
    },
    staleTime: 30 * 60 * 1000,    // 30 minutes
    gcTime: 24 * 60 * 60 * 1000,  // 24 hours
    refetchOnReconnect: true,
  });
}

// hooks/use-offline-transaction.ts
// Creates transactions locally first, queues for sync

export function useCreateOfflineTransaction() {
  const { status } = useConnectivity();
  
  return useMutation({
    mutationFn: async (data: CreateTransactionInput) => {
      const db = await getDB();
      const tempId = generateUlid();
      
      const localTx: LocalTransaction = {
        tempId,
        syncStatus: 'pending',
        transactionNo: generateLocalTransactionNo(data.branchCode),
        ...data,
        createdAt: new Date().toISOString(),
      };
      
      // Save to local store
      await db.put('localTransactions', localTx);
      
      // Add to sync queue
      await db.put('syncQueue', {
        id: generateUlid(),
        operation: 'create_transaction',
        payload: localTx,
        tempId,
        status: 'pending',
        retryCount: 0,
        maxRetries: 5,
        createdAt: new Date().toISOString(),
      });
      
      return localTx;
    },
  });
}
```

---

## API Endpoints (New/Modified)

| Method | Route | Description |
|---|---|---|
| `POST` | `/transactions/offline-sync` | Sync an offline-created transaction (idempotent via OfflineTempId) |
| `GET` | `/pos/cache-bundle` | Returns all reference data in one call: services, pricing, employees, sizes, vehicle types, merchandise. Reduces round trips during cache refresh. |
| `GET` | `/health` | Simple health check for connectivity ping (already exists) |

### Cache Bundle Endpoint

Instead of 6 separate API calls to refresh the cache, one endpoint returns everything the POS needs:

```csharp
// GET /api/v1/pos/cache-bundle?branchId={branchId}
// Returns:
{
  services: [...],         // With full pricing + commission matrices
  servicePackages: [...],  // With package pricing
  employees: [...],        // Active, branch-filtered
  sizes: [...],
  vehicleTypes: [...],
  merchandise: [...],
  tenantPlan: {...},
  serverTime: "2026-03-29T14:30:00Z",  // For clock sync
  cacheVersion: "v42"                   // Increment when data changes
}
```

The POS calls this on app load and every 30 minutes. If `cacheVersion` hasn't changed, the server can return 304 Not Modified (no body, saves bandwidth).

---

## Claude Code Prompts — Phase 19

### Prompt 19.1 — PWA + Service Worker Setup

```
Add PWA and offline infrastructure to the POS app (apps/pos/):

1. Install: serwist, @serwist/next, idb
2. Configure Serwist in next.config.ts
3. Create sw.ts service worker with:
   - Precache manifest for app shell
   - Runtime caching: StaleWhileRevalidate for API reference data
   - CacheFirst for fonts
   - Offline fallback page
4. Create public/manifest.json with SplashSphere POS identity
5. Create /offline page — friendly message with link to POS home
6. Register service worker in root layout
7. Create PWA icons (192px, 512px, maskable) — placeholder blue squares with "SS"
8. Add <meta name="theme-color"> and <link rel="manifest"> to layout

Test: build the POS app, serve with `next start`, verify:
- App is installable (Chrome shows install prompt)
- Offline fallback page loads when navigating offline
- Static assets served from cache on reload
```

### Prompt 19.2 — IndexedDB Offline Store

```
Build the IndexedDB offline data layer for POS:

lib/offline/db.ts:
- Define SplashSpherePOSDB schema with all stores from the spec
- Export getDB() function
- Include all interfaces: LocalTransaction, LocalCashMovement, 
  LocalShift, SyncQueueItem, CachedService, etc.

lib/offline/cache-manager.ts:
- refreshCache(): fetches /api/v1/pos/cache-bundle and populates all stores
- getCachedServices(): reads from IndexedDB services store
- getCachedEmployees(branchId): reads from IndexedDB with branch index
- lookupVehicle(plateNumber): checks IndexedDB first, falls back to API
- lookupCustomer(query): checks IndexedDB first, falls back to API
- cacheVehicle(vehicle): saves to IndexedDB after API lookup
- clearCache(): wipes all stores (for sign-out)
- getCacheAge(): returns time since last sync

lib/offline/connectivity.ts:
- useConnectivity() hook: tracks online/offline/reconnecting status
  Uses both navigator.onLine events AND periodic /health ping
- usePendingSyncCount() hook: counts unsynced items

Backend:
- Add GET /api/v1/pos/cache-bundle endpoint that returns all reference 
  data in one response. Include cacheVersion for conditional requests.
- Add OfflineTempId field to Transaction entity.

Configure cache refresh: on app load + every 30 minutes while online.
```

### Prompt 19.3 — Offline Transaction Flow + Sync Engine

```
Build the offline transaction creation and sync engine:

lib/offline/sync-engine.ts:
- SyncEngine class: processQueue() runs every 10 seconds when online
- Picks up 'pending' items from syncQueue IndexedDB store
- Executes operations: create_transaction, create_cash_movement, 
  open_shift, close_shift
- On success: reconcile (update local records with server IDs)
- On failure: increment retryCount, mark as 'failed' after maxRetries
- On 409 Conflict: mark as 'conflict' for manual resolution
- Idempotency: transactions include OfflineTempId

hooks/use-create-offline-transaction.ts:
- Creates LocalTransaction with ULID tempId
- Generates local transaction number
- Saves to IndexedDB localTransactions store
- Adds to syncQueue
- Returns immediately (optimistic)

hooks/use-offline-services.ts:
- Reads from IndexedDB first, refreshes from API when online
- Includes pricing matrix data for offline price resolution

hooks/use-offline-employees.ts:
- Same pattern: IndexedDB first, API refresh when online

Backend:
- Add POST /api/v1/transactions/offline-sync endpoint
  Accepts OfflineTempId, checks for duplicates, processes normally
  Returns server ID + authoritative transactionNo

Update the POS transaction screen:
- Use offline hooks instead of direct API calls
- Transaction completion saves locally first, queues for sync
- Show syncStatus indicator on each transaction in history
```

### Prompt 19.4 — Offline UI Integration

```
Integrate offline support into the POS UI:

1. Connection status in POS top bar:
   - Green dot: online
   - Red dot + amber banner: "Offline — transactions saved locally"
   - Yellow pulsing dot: reconnecting
   - Green dot + count: "Syncing 3 items..."
   - Red badge: "2 items failed to sync" (tappable)

2. Transaction screen:
   - Works identically online and offline
   - After completion, show sync indicator:
     Online: "✓ Saved" (green)
     Offline: "⏳ Saved locally — will sync when online" (amber)
   - Receipt generates from local data (no server needed)

3. Transaction history:
   - Show sync status badge on each transaction:
     synced (green check), pending (amber clock), failed (red X)
   - Filter: "Show unsynced only"
   - Failed items: tap to retry or view conflict details

4. Shift management:
   - Open shift works offline (saved locally, queued for sync)
   - Cash movements work offline
   - Close shift: denomination count works offline
   - Report generated from local data
   - All queued for sync

5. Conflict resolution dialog:
   - Shows when sync returns 409
   - Displays: offline data vs server expectation
   - Actions: "Keep mine" / "Accept server" / "Ask manager"

6. Offline fallback page (/offline):
   - Friendly message: "You're offline but the POS is still working"
   - Link to transaction screen
   - Show cache age: "Data last synced 15 min ago"

7. Cache age indicator (subtle):
   - In POS settings or footer: "Last synced: 3 min ago"
   - Turns amber when > 30 min stale
   - Turns red when > 2 hours stale
```

---

## Testing Offline

### Manual Testing Checklist

- [ ] Open POS, go to Chrome DevTools → Network → check "Offline"
- [ ] Create a transaction with 2 services, 3 employees, cash payment → succeeds
- [ ] Transaction appears in local history with "pending sync" badge
- [ ] Receipt prints correctly from local data
- [ ] Uncheck "Offline" → transaction syncs within 10 seconds
- [ ] Transaction badge changes to "synced"
- [ ] Open POS, sign in, let it cache data, go offline, reload page → app loads from cache
- [ ] Vehicle plate lookup works from cached data
- [ ] Record a cash movement offline → syncs when online
- [ ] Create 5 transactions offline, go online → all 5 sync in order
- [ ] Create a transaction offline, then create the same on another terminal online → server handles duplicate gracefully

### Automated Testing

```typescript
// Test with Playwright: page.context().setOffline(true)
test('creates transaction while offline', async ({ page, context }) => {
  // Cache data while online
  await page.goto('/');
  await page.waitForTimeout(2000); // Let cache populate
  
  // Go offline
  await context.setOffline(true);
  
  // Create transaction
  await page.fill('[data-testid="plate-input"]', 'ABC-1234');
  // ... select services, employees, payment
  await page.click('[data-testid="complete-btn"]');
  
  // Verify local save
  await expect(page.locator('[data-testid="sync-badge"]')).toHaveText('Pending');
  
  // Go online
  await context.setOffline(false);
  await page.waitForTimeout(15000); // Wait for sync
  
  // Verify synced
  await expect(page.locator('[data-testid="sync-badge"]')).toHaveText('Synced');
});
```

---

## Phase Summary

| Prompt | What | Layer |
|---|---|---|
| 19.1 | PWA setup: Serwist service worker, manifest, offline page, icons | Frontend (POS) |
| 19.2 | IndexedDB schema, cache manager, connectivity hooks, cache bundle API | Full stack |
| 19.3 | Offline transaction flow, sync engine, idempotent sync endpoint | Full stack |
| 19.4 | Offline UI: status indicators, sync badges, conflict dialog, cache age | Frontend (POS) |

**Total: 4 prompts in Phase 19.**

---

## Key Design Decisions

1. **IndexedDB over localStorage.** localStorage has a 5MB limit and is synchronous (blocks the main thread). IndexedDB supports structured data, indexes, and hundreds of MB. The `idb` library provides a clean Promise-based API.

2. **Cache bundle endpoint.** One API call to refresh all reference data instead of 6 separate calls. Reduces latency and simplifies cache logic. Supports conditional requests (304 Not Modified) to save bandwidth.

3. **ULID for local IDs.** ULIDs are globally unique (like UUIDs) but also sortable by time. This means locally-generated IDs won't collide with server IDs, and transactions stay in chronological order.

4. **Idempotent sync via OfflineTempId.** If the sync request succeeds but the response is lost (network drops during response), the POS will retry. The server checks OfflineTempId and returns the existing record instead of creating a duplicate.

5. **Prices captured at time of sale.** If a service price changes on the server while the POS is offline, the transaction keeps the price the customer was quoted. The server logs a discrepancy event for admin review, but doesn't reject the transaction.

6. **GCash/Card payments are "pending verification" offline.** The payment is recorded locally, but the actual gateway call happens during sync. The admin can see which payments need verification. This is a business decision: some owners might prefer to block non-cash payments offline — make it a tenant setting.

7. **8-hour maximum offline session.** After 8 hours without server contact, the POS stops accepting new transactions. This prevents indefinite offline operation without authentication verification, while still covering a full work shift during an outage.

8. **Admin app is NOT offline.** Offline complexity is expensive to build. The admin dashboard is used by owners/managers who typically have better connectivity. Only the POS (used by cashiers in the car wash bay area) needs offline support.
