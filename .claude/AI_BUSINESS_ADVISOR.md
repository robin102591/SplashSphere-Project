# SplashSphere — AI Business Advisor (Negosyo AI)

> **Phase:** 21. Requires: Phases 15 (expenses, loyalty, shifts), 16 (subscriptions), and the core transaction/payroll system.
> **Plan gating:** Growth+ feature. Enterprise gets unlimited queries. Growth gets 50 queries/day.
> **Branding:** Internally called "Negosyo AI" — "negosyo" is Filipino for "business." The owner sees it as their personal business advisor who understands car wash operations.

---

## Why This Matters

A Philippine car wash owner opens the SplashSphere dashboard. They see charts, tables, numbers — revenue is ₱32,450, commissions are ₱8,920, they have 47 transactions. But what does that *mean*?

- Is that good compared to last week?
- Are they actually profitable after expenses?
- Which service should they promote?
- Should they raise the XL pricing?
- Why did Saturday revenue drop?
- Is that new employee performing well?
- When will they run out of soap?

They don't have a finance degree. They don't know how to read a P&L report. They need someone to **tell them** — in plain language, in the context of *their* business — what's happening and what to do about it.

That's Negosyo AI. Not a chatbot that answers generic questions. A business advisor that knows their data, understands car wash operations, speaks their language, and gives specific, actionable advice.

---

## Architecture

```
┌──────────────────────────────────────────────────────────────┐
│  ADMIN DASHBOARD (Next.js)                                    │
│                                                              │
│  ┌───────────────────────────────────────────────────────┐   │
│  │ Negosyo AI Panel                                       │   │
│  │                                                       │   │
│  │ "How's my business doing this week?"                  │   │
│  │                                                       │   │
│  │ 💬 AI: "Your revenue is up 12% vs last week at       │   │
│  │     ₱187,450. However, your net profit margin         │   │
│  │     dropped from 34% to 28% because soap expenses    │   │
│  │     increased by ₱3,200. I recommend checking your   │   │
│  │     soap supplier pricing — you're paying ₱195/L     │   │
│  │     vs the ₱180 you paid last month."                 │   │
│  └───────────────────────────────────────────────────────┘   │
│                              │                               │
│                    POST /api/v1/ai/chat                       │
│                              │                               │
├──────────────────────────────▼───────────────────────────────┤
│  .NET API                                                    │
│                                                              │
│  ┌─────────────────┐    ┌──────────────────────────────┐    │
│  │ AI Chat Handler  │───▶│ Data Functions (Tools)        │    │
│  │                  │    │                              │    │
│  │ Calls Anthropic  │    │ GetRevenueSummary()          │    │
│  │ API with tools   │    │ GetProfitLoss()              │    │
│  │                  │    │ GetEmployeePerformance()     │    │
│  │ Claude Sonnet    │    │ GetTopServices()             │    │
│  │ 4.6 processes    │    │ GetCashFlowForecast()        │    │
│  │ with business    │    │ GetCustomerRetention()       │    │
│  │ context          │    │ GetInventoryStatus()         │    │
│  │                  │    │ GetShiftVarianceHistory()    │    │
│  │                  │    │ GetPayrollSummary()          │    │
│  │                  │    │ ComparePerformance()         │    │
│  └─────────────────┘    └──────────────────────────────┘    │
│           │                         │                        │
│           ▼                         ▼                        │
│  ┌─────────────────┐    ┌──────────────────────────────┐    │
│  │ Anthropic API    │    │ SplashSphereDbContext         │    │
│  │ (Claude Sonnet   │    │ (tenant-scoped queries)       │    │
│  │  4.6)            │    │                              │    │
│  └─────────────────┘    └──────────────────────────────┘    │
└──────────────────────────────────────────────────────────────┘
```

**How it works:**

1. Owner types a question or the system generates a proactive insight
2. Backend builds a message with system prompt + tenant context + available tools
3. Calls the Anthropic API (Claude Sonnet 4.6) with tool definitions
4. Claude decides which data functions to call based on the question
5. Backend executes the tool calls against the tenant's data (tenant-scoped, never cross-tenant)
6. Claude receives the data and generates a natural language response with specific numbers, comparisons, and recommendations
7. Response streamed back to the frontend

**Why Claude Sonnet 4.6, not Opus?** Sonnet is fast and affordable for high-frequency business queries. Opus is overkill for "how's my revenue?" questions. For daily briefs and deep analysis, you can optionally use Opus.

---

## Three Delivery Channels

### Channel 1: Conversational Chat (Ask Anything)

A chat panel in the admin dashboard sidebar or a dedicated `/ai` page. The owner types natural language questions and gets contextual answers.

**Example conversations:**

```
Owner: "How's my business doing?"

Negosyo AI: "Here's your snapshot for this week (Mar 18-24):

  Revenue: ₱187,450 (▲ 12% vs last week)
  Transactions: 312 (▲ 8%)
  Avg ticket: ₱601 (▲ 3.6%)
  Net profit: ₱52,486 (margin: 28%)
  
  Your revenue is strong, but your margin dropped from 34% last week to 28% 
  this week. The main driver: soap expenses jumped ₱3,200 (you used 18L more 
  than usual). Check if your employees are over-diluting or if there's waste.
  
  Top performer: Juan D. earned ₱4,280 in commission from 38 services.
  Concern: Your Cebu branch did 40% fewer transactions than Makati — 
  is there a staffing issue?"
```

```
Owner: "Should I raise the price for XL vehicles?"

Negosyo AI: "Based on your data:

  XL vehicles (Hiace, trucks) make up 8% of your transactions but 
  consume 2.3x more soap and water than Medium vehicles.
  
  Current pricing: Basic Wash XL = ₱350
  Supply cost per XL wash: ₱42
  Commission (3 employees): ₱60
  Gross margin: ₱248 (71%)
  
  Compare to Medium:
  Basic Wash Medium = ₱220
  Supply cost: ₱19
  Commission: ₱40
  Gross margin: ₱161 (73%)
  
  Your XL margin is 2% lower than Medium. A ₱30-50 increase to ₱380-400 
  would bring XL margins in line. Your competitors in Makati charge ₱380-420 
  for XL basic wash.
  
  Recommendation: Raise XL Basic Wash to ₱380. Monitor volume for 2 weeks."
```

```
Owner: "Who's my best employee?"

Negosyo AI: "Ranking by total commission earned this month:

  1. Juan Dela Cruz — ₱12,480 (142 services, ₱87.89 avg)
  2. Pedro Santos — ₱10,920 (128 services, ₱85.31 avg)  
  3. Maria Garcia — ₱9,640 (118 services, ₱81.69 avg)
  
  But if you rank by efficiency (commission per hour worked):
  1. Pedro Santos — ₱182/hour (works fewer hours but very productive)
  2. Juan Dela Cruz — ₱156/hour
  3. Maria Garcia — ₱148/hour
  
  Pedro is your most efficient. Juan brings the most volume. 
  Consider pairing Maria with Pedro — she learns from his speed, 
  and they cover more cars together."
```

```
Owner: "Kailan mauubos ang sabon?" (When will the soap run out?)

Negosyo AI: "Based on your current usage rate:

  Car Wash Soap: 12 liters remaining
  Average daily usage: 2.8 liters/day
  Estimated days until empty: 4 days (by March 28)
  
  Your last purchase was 20L at ₱195/L from Manila Chemicals.
  
  Recommendation: Reorder today. At your volume, a 25L drum 
  would last 9 days. Ask your supplier about bulk pricing — 
  20L drums were ₱180/L three months ago."
```

### Channel 2: Daily Business Brief (Proactive)

Every morning at 8 AM (before the shift starts), the system generates a brief and shows it as a card on the dashboard. Optionally sent via SMS.

```
┌─ 📊 Good morning, Juan! Here's your daily brief ──────────────┐
│                                                                │
│  YESTERDAY (March 24, 2026) — Makati Branch                   │
│                                                                │
│  Revenue: ₱32,450 (▲ 5% vs same day last week)               │
│  Transactions: 47 | Avg ticket: ₱690                          │
│  Net Profit: ₱9,120 (28% margin)                              │
│  Top Service: Premium Wash (18 transactions)                   │
│  Top Employee: Juan D.C. (₱1,280 commission)                  │
│                                                                │
│  ⚡ ALERTS                                                     │
│  🔴 Soap supply at 12L — reorder needed in 4 days             │
│  🟡 Saturday revenue was 23% below average — weather related?  │
│  🟢 Customer Maria Santos reached Gold tier — 12th visit       │
│                                                                │
│  💡 INSIGHT                                                    │
│  "Your Wax & Polish service has grown 35% month-over-month.    │
│   Consider creating a 'Premium Detail' package combining       │
│   Wax + Interior Vacuum + Tire Shine at a 10% bundle discount. │
│   Based on your data, this could add ~₱8,500/week in revenue." │
│                                                                │
│                                       [Ask Negosyo AI more →]  │
└────────────────────────────────────────────────────────────────┘
```

### Channel 3: Smart Alerts (Anomaly Detection)

Push notifications (in-app + optional SMS) when the AI detects something unusual.

**Alert Types:**

| Alert | Trigger | Example |
|---|---|---|
| Revenue drop | Daily revenue > 20% below 30-day average | "Monday revenue was ₱18,200 — 28% below your average of ₱25,300. This is unusual for a Monday." |
| Cash variance pattern | Cashier has negative variance > 3 consecutive shifts | "Ana R. has been short ₱80-150 on her last 3 shifts. Total shortage: ₱340. Review recommended." |
| Employee performance change | Employee's weekly commission drops > 30% | "Pedro's commission dropped from ₱3,200 to ₱1,980 this week. Is he on reduced hours or underperforming?" |
| Low stock forecast | Supply will hit zero within 3 days at current usage | "Car Wash Soap will run out by Thursday at current usage rate. Reorder now." |
| Customer churn risk | Regular customer hasn't visited in 30+ days | "5 Gold/Platinum customers haven't visited in 30+ days. Consider sending a 'we miss you' SMS with a discount." |
| Pricing opportunity | A service's margin is significantly lower than others | "Your Undercarriage Wash has only 58% margin vs 73% average. Supply cost is high — consider raising price by ₱30." |
| Peak hour insight | Clear revenue patterns by hour of day | "72% of your revenue happens between 8 AM and 2 PM. Consider offering a 10% 'afternoon special' after 2 PM to spread demand." |
| Payroll anomaly | Weekly payroll total varies > 20% from 4-week average | "This week's payroll is ₱48,200 vs your 4-week average of ₱38,500. Check if overtime or bonus entries are correct." |

---

## Data Functions (Tools for Claude)

These are .NET services that query the tenant's data and return structured results. Claude calls them via the Anthropic API tool use feature.

```csharp
public interface IBusinessDataService
{
    // Revenue & Transactions
    Task<RevenueSummary> GetRevenueSummary(string tenantId, string? branchId, 
        DateRange period, DateRange? comparePeriod);
    Task<List<DailyRevenue>> GetRevenueTimeline(string tenantId, string? branchId,
        DateRange period);
    Task<RevenueByService> GetTopServices(string tenantId, string? branchId,
        DateRange period, int top);
    Task<RevenueByPaymentMethod> GetPaymentMethodBreakdown(string tenantId,
        string? branchId, DateRange period);
    Task<RevenueByHour> GetPeakHourAnalysis(string tenantId, string? branchId,
        DateRange period);

    // Profitability
    Task<ProfitLossStatement> GetProfitLoss(string tenantId, string? branchId,
        DateRange period);
    Task<CostPerWashBreakdown> GetCostPerWash(string tenantId, string serviceId);
    Task<MarginAnalysis> GetMarginByService(string tenantId, string? branchId,
        DateRange period);

    // Employees
    Task<List<EmployeePerformance>> GetEmployeePerformance(string tenantId,
        string? branchId, DateRange period, string sortBy);
    Task<EmployeeComparison> CompareEmployees(string tenantId, 
        string[] employeeIds, DateRange period);
    Task<PayrollSummary> GetPayrollSummary(string tenantId, DateRange period);

    // Customers
    Task<CustomerRetentionMetrics> GetCustomerRetention(string tenantId,
        DateRange period);
    Task<List<ChurnRiskCustomer>> GetChurnRiskCustomers(string tenantId, 
        int inactiveDays);
    Task<LoyaltyProgramMetrics> GetLoyaltyMetrics(string tenantId);
    Task<CustomerSegmentation> GetCustomerSegments(string tenantId);

    // Inventory
    Task<InventoryStatus> GetInventoryStatus(string tenantId, string? branchId);
    Task<List<StockForecast>> GetStockForecasts(string tenantId, string? branchId,
        int forecastDays);
    Task<SupplyUsageTrend> GetSupplyUsageTrend(string tenantId, 
        string supplyItemId, DateRange period);

    // Operations
    Task<ShiftVarianceHistory> GetShiftVarianceHistory(string tenantId,
        string? cashierId, DateRange period);
    Task<QueueMetrics> GetQueueMetrics(string tenantId, string? branchId,
        DateRange period);
    Task<BranchComparison> CompareBranches(string tenantId, DateRange period);

    // Trends & Forecasting
    Task<RevenueForecast> ForecastRevenue(string tenantId, int forecastDays);
    Task<SeasonalPattern> GetSeasonalPatterns(string tenantId);
    Task<List<Anomaly>> DetectAnomalies(string tenantId, DateRange period);
}
```

### Tool Definitions for Anthropic API

```csharp
private static readonly List<Tool> BusinessTools = [
    new Tool
    {
        Name = "get_revenue_summary",
        Description = "Get revenue, transaction count, and averages for a period. " +
            "Can compare to a previous period. Use this when the owner asks about " +
            "sales, revenue, income, or how business is doing.",
        InputSchema = new {
            type = "object",
            properties = new {
                branch_id = new { type = "string", description = "Optional branch filter" },
                period = new { type = "string", description = "today, yesterday, this_week, last_week, this_month, last_month, last_7_days, last_30_days, or custom date range" },
                compare_to = new { type = "string", description = "Optional comparison period" }
            }
        }
    },
    new Tool
    {
        Name = "get_profit_loss",
        Description = "Get a complete profit & loss statement: revenue, COGS " +
            "(merchandise + supply usage), expenses by category, commissions, " +
            "and net profit. Use this when the owner asks about profit, margin, " +
            "expenses, costs, or whether they're making money.",
        InputSchema = new {
            type = "object",
            properties = new {
                branch_id = new { type = "string" },
                period = new { type = "string" }
            },
            required = new[] { "period" }
        }
    },
    new Tool
    {
        Name = "get_employee_performance",
        Description = "Get employee performance metrics: services completed, " +
            "commission earned, avg commission per service, hours worked, " +
            "commission per hour. Use when the owner asks about employees, " +
            "who's the best, performance, or staffing.",
        InputSchema = new {
            type = "object",
            properties = new {
                branch_id = new { type = "string" },
                period = new { type = "string" },
                sort_by = new { type = "string", @enum = new[] { 
                    "commission_total", "services_count", "commission_per_hour", 
                    "avg_commission" } },
                top = new { type = "integer", description = "Number of employees to return" }
            }
        }
    },
    new Tool
    {
        Name = "get_inventory_status",
        Description = "Get current stock levels for all supplies and merchandise. " +
            "Includes estimated days until reorder needed. Use when the owner asks " +
            "about inventory, supplies, stock, soap, towels, or running out.",
        InputSchema = new {
            type = "object",
            properties = new {
                branch_id = new { type = "string" }
            }
        }
    },
    new Tool
    {
        Name = "get_cost_per_wash",
        Description = "Get the supply cost breakdown for a specific service by " +
            "vehicle size: what supplies are consumed, cost per unit, total " +
            "cost per wash, and margin. Use when the owner asks about pricing, " +
            "margins, whether a service is profitable, or supply costs.",
        InputSchema = new {
            type = "object",
            properties = new {
                service_name = new { type = "string", description = "Service name or ID" }
            },
            required = new[] { "service_name" }
        }
    },
    new Tool
    {
        Name = "forecast_stock_depletion",
        Description = "Predict when a supply item will run out based on recent " +
            "usage rate. Use when the owner asks 'when will X run out' or " +
            "'do I need to reorder'.",
        InputSchema = new {
            type = "object",
            properties = new {
                supply_name = new { type = "string" },
                branch_id = new { type = "string" }
            }
        }
    },
    new Tool
    {
        Name = "get_shift_variance_history",
        Description = "Get cash variance history per cashier over time. " +
            "Shows patterns of over/short. Use when the owner asks about " +
            "cash shortages, cashier accuracy, missing money, or trust issues.",
        InputSchema = new {
            type = "object",
            properties = new {
                cashier_name = new { type = "string" },
                period = new { type = "string" }
            }
        }
    },
    new Tool
    {
        Name = "compare_branches",
        Description = "Compare performance across branches: revenue, transactions, " +
            "avg ticket, employee count, top services. Use when the owner asks " +
            "about branch comparison or which branch is doing better.",
        InputSchema = new {
            type = "object",
            properties = new {
                period = new { type = "string" }
            },
            required = new[] { "period" }
        }
    },
    new Tool
    {
        Name = "get_customer_retention",
        Description = "Get customer retention metrics: return rate, avg visit " +
            "frequency, churn risk list, loyalty tier distribution. Use when " +
            "the owner asks about customers, retention, loyalty, or churn.",
        InputSchema = new {
            type = "object",
            properties = new {
                period = new { type = "string" }
            }
        }
    },
    new Tool
    {
        Name = "detect_anomalies",
        Description = "Scan for unusual patterns in business data: revenue " +
            "spikes/drops, expense anomalies, employee performance changes, " +
            "stock consumption changes. Use when the owner asks 'anything " +
            "unusual' or 'what should I know'.",
        InputSchema = new {
            type = "object",
            properties = new {
                period = new { type = "string" }
            }
        }
    },
    new Tool
    {
        Name = "get_peak_hours",
        Description = "Analyze revenue and transaction patterns by hour of day " +
            "and day of week. Use when the owner asks about busy times, " +
            "staffing scheduling, or when to run promotions.",
        InputSchema = new {
            type = "object",
            properties = new {
                branch_id = new { type = "string" },
                period = new { type = "string" }
            }
        }
    }
];
```

---

## System Prompt

The system prompt shapes Claude's persona as a Filipino car wash business advisor:

```
You are Negosyo AI, the business advisor built into SplashSphere — a car wash 
management platform used in the Philippines.

You are speaking to a car wash owner or manager. They may not have business or 
finance expertise. Your job is to:

1. Answer their questions about their business using REAL DATA from their system.
   Always call the appropriate data functions — never guess or make up numbers.

2. Explain insights in simple, conversational language. Avoid jargon.
   Say "you made ₱32,450" not "aggregate revenue totaled ₱32,450."

3. Always provide CONTEXT — compare to last week, last month, or industry averages.
   Raw numbers without comparison are meaningless.

4. Give SPECIFIC, ACTIONABLE recommendations. Don't say "consider optimizing."
   Say "raise your XL Basic Wash price from ₱350 to ₱380."

5. When you see a problem, explain the cause AND the fix.
   "Your margin dropped because soap costs went up" + "check your supplier pricing."

6. Understand Philippine car wash operations:
   - Commission-based employees (percentage or fixed per service)
   - Weekly payroll cutoff every Saturday
   - Cash, GCash, and card payments
   - Vehicle pricing by type (Sedan, SUV, Van) and size (Small, Medium, Large, XL)
   - Supply consumption varies by vehicle size
   - Peak hours: Saturday-Sunday, 8 AM - 2 PM
   - Rainy season reduces volume but doesn't stop operations

7. You can understand and respond in Filipino/Tagalog if the owner writes in Filipino.
   Mix Taglish naturally if appropriate (how real Filipino business owners communicate).

8. Format responses for readability:
   - Use ₱ for all peso amounts
   - Use bullet points for lists
   - Bold key numbers and recommendations
   - Keep responses under 300 words unless the owner asks for a deep dive

9. You are NOT a general-purpose AI assistant. Stay focused on this owner's car wash 
   business data. If asked about unrelated topics, politely redirect: 
   "I'm your car wash business advisor — I can help with your revenue, employees, 
   inventory, and operations. What would you like to know?"

10. Never reveal raw SQL, API details, or system internals. You're a business advisor, 
    not a developer tool.

Current tenant: {tenantName}
Current branch context: {branchName ?? "All branches"}
Current date/time: {DateTime.Now:MMMM dd, yyyy h:mm tt} (Philippine Time)
Owner name: {ownerName}
Plan: {planTier}
```

---

## API Endpoints

| Method | Route | Description |
|---|---|---|
| `POST` | `/ai/chat` | Send a message, get a response (streaming) |
| `GET` | `/ai/conversations` | List past conversations (paginated) |
| `GET` | `/ai/conversations/{id}` | Get conversation history |
| `DELETE` | `/ai/conversations/{id}` | Delete a conversation |
| `GET` | `/ai/daily-brief` | Get today's daily brief (generated or cached) |
| `GET` | `/ai/alerts` | Get active smart alerts |
| `PATCH` | `/ai/alerts/{id}/dismiss` | Dismiss an alert |
| `GET` | `/ai/usage` | Get AI query count for current billing period |

### Chat Request/Response

```typescript
// POST /ai/chat
{
  conversationId?: string,  // null for new conversation
  message: string,          // "How's my business doing?"
  branchId?: string         // optional branch filter
}

// Response (Server-Sent Events for streaming)
event: message
data: {"type": "text", "content": "Here's your snapshot for this week..."}

event: message
data: {"type": "text", "content": "\n\n**Revenue:** ₱187,450 (▲ 12%)"}

event: tool_call
data: {"type": "tool_call", "name": "get_revenue_summary", "status": "running"}

event: tool_result  
data: {"type": "tool_result", "name": "get_revenue_summary", "status": "complete"}

event: done
data: {"conversationId": "conv_abc123", "tokensUsed": 2340}
```

---

## Frontend Implementation

### AI Chat Panel

Two presentation options (let the tenant choose in settings):

**Option A: Sidebar Panel** — A sliding panel on the right side of the admin dashboard. Always accessible via a floating "💬 Ask Negosyo AI" button. Doesn't navigate away from the current page.

**Option B: Dedicated Page** — `/ai` page with full-width chat interface. More room for responses and data visualizations.

```
┌─ Negosyo AI ──────────────────────────────────────────────┐
│                                                            │
│  💬 Good morning, Juan! I'm your business advisor.         │
│     Ask me anything about your car wash.                   │
│                                                            │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ You: "How's my business doing this week?"             │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                            │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ 🤖 Negosyo AI:                                       │  │
│  │                                                      │  │
│  │ Your revenue is up 12% vs last week at ₱187,450.    │  │
│  │                                                      │  │
│  │ However, your net profit margin dropped from 34%     │  │
│  │ to 28% because soap expenses increased by ₱3,200.   │  │
│  │                                                      │  │
│  │ **Recommendation:** Check your soap supplier         │  │
│  │ pricing — you're paying ₱195/L vs ₱180 last month.  │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                            │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Suggested questions:                                  │  │
│  │ [Who's my best employee?] [Am I profitable?]          │  │
│  │ [When will soap run out?] [Compare my branches]       │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                            │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ 💬 Type your question...                    [Send ↗]  │  │
│  └──────────────────────────────────────────────────────┘  │
└────────────────────────────────────────────────────────────┘
```

**Suggested Questions** — shown below the chat when the conversation starts or after each response. Contextual to recent data:
- After showing revenue: "Why did revenue drop on Saturday?"
- After showing employees: "How does Juan compare to Pedro?"
- After showing inventory: "When should I reorder towels?"
- Generic starters: "How's my business?", "Am I profitable?", "Any issues I should know about?"

### Daily Brief Card

On the admin dashboard home page, above the KPI cards:

```
┌─ 📊 Daily Brief — March 25, 2026 ─────────────────── [×] ─┐
│                                                              │
│  Yesterday: ₱32,450 revenue (▲ 5% vs last Tuesday)          │
│  47 transactions | ₱690 avg ticket | Net profit: ₱9,120     │
│                                                              │
│  ⚡ Soap at 12L — reorder in 4 days                          │
│  ⚡ Saturday revenue was 23% below avg                        │
│  💡 "Your Wax & Polish service grew 35% this month —         │
│     consider bundling it into a new package"                  │
│                                                              │
│                         [Ask more →] [View full brief →]     │
└──────────────────────────────────────────────────────────────┘
```

Generated by a Hangfire job at 6 AM daily. Cached and served when the owner opens the dashboard.

### Smart Alert Badges

In the notification bell (admin header), AI alerts appear alongside other notifications:

```
🔔 (3)
├── 🔴 AI: Soap supply critical — 4 days remaining
├── 🟡 AI: Saturday revenue 23% below average
├── 🟡 AI: 5 Gold customers haven't visited in 30 days
```

Dismissible. Tapping opens the AI chat pre-filled with a follow-up question.

---

## Conversation Memory

Each conversation is stored so the owner can continue a thread:

```csharp
public sealed class AiConversation : IAuditableEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string? Title { get; set; }              // Auto-generated from first message
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<AiMessage> Messages { get; set; } = [];
}

public sealed class AiMessage
{
    public string Id { get; set; } = string.Empty;
    public string ConversationId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;   // "user" or "assistant"
    public string Content { get; set; } = string.Empty;
    public int? TokensUsed { get; set; }
    public List<string>? ToolsCalled { get; set; }      // Which data functions were used
    public DateTime CreatedAt { get; set; }
}

public sealed class AiDailyBrief
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string? BranchId { get; set; }
    public DateTime BriefDate { get; set; }
    public string Content { get; set; } = string.Empty; // Markdown
    public string? AlertsJson { get; set; }             // Smart alerts included
    public DateTime GeneratedAt { get; set; }
}

public sealed class AiSmartAlert
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string AlertType { get; set; } = string.Empty; // revenue_drop, stock_low, etc.
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;  // info, warning, critical
    public bool IsDismissed { get; set; }
    public string? SuggestedQuestion { get; set; }         // Pre-fill for AI chat
    public DateTime CreatedAt { get; set; }
}
```

---

## Usage Limits & Cost Control

### Per-Plan Limits

| Plan | AI Queries/Day | Daily Brief | Smart Alerts | Conversation History |
|---|---|---|---|---|
| Starter | ✗ (not available) | ✗ | ✗ | ✗ |
| Growth | 50 queries/day | ✓ | ✓ (top 5) | 7 days |
| Enterprise | Unlimited | ✓ | ✓ (all) | 90 days |

### Cost Estimation

Using Claude Sonnet 4.6 pricing:
- Average query: ~2,000 input tokens (system prompt + tool results) + ~500 output tokens
- Estimated cost per query: ~$0.01
- 50 queries/day per Growth tenant: ~$0.50/day = ~$15/month
- Daily brief generation: ~$0.02 per tenant per day = ~$0.60/month

At 50 Growth tenants: 50 × $15 = $750/month in API costs. This is well within the ₱2,999/month ($54) subscription revenue per tenant margin, especially since most tenants won't use all 50 queries daily.

### Token Optimization

- Cache data function results for 5 minutes (same question within 5 min returns cached data)
- Daily brief is generated once per tenant per day, not on-demand
- Smart alerts are generated by a batch Hangfire job, not per-request
- Truncate conversation history to last 10 messages when sending to the API
- Use concise tool result formats (structured data, not verbose)

---

## Hangfire Jobs

| Job | Schedule | Description |
|---|---|---|
| `GenerateDailyBriefs` | Daily 6 AM PHT | For each active Growth+ tenant: query yesterday's data, call Claude to generate brief, cache result |
| `DetectSmartAlerts` | Every 4 hours | For each active Growth+ tenant: run anomaly detection queries, generate alerts for significant findings |
| `CleanupConversations` | Weekly Sunday | Delete conversations older than retention period (7d Growth, 90d Enterprise) |
| `TrackAiUsage` | Hourly | Aggregate query counts per tenant for billing/limits |

---

## Claude Code Prompts — Phase 21

### Prompt 21.1 — AI Infrastructure + Data Functions

```
Add the Negosyo AI infrastructure to SplashSphere:

Domain/Entities/:
- AiConversation, AiMessage, AiDailyBrief, AiSmartAlert

Infrastructure/Persistence/Configurations/:
- EF configs for all AI entities. Tenant-scoped global filters.
- Migration: "AddNegosyoAi"

Infrastructure/ExternalServices/AnthropicService.cs:
- IAnthropicService interface in Application/Interfaces/
- Implementation that calls the Anthropic Messages API (Claude Sonnet 4.6)
- Supports tool use: sends tool definitions, handles tool_use responses,
  calls data functions, returns tool_result, gets final response
- Streaming support via Server-Sent Events
- Env vars: Anthropic__ApiKey, Anthropic__Model (default: claude-sonnet-4-6)

Application/Services/BusinessDataService.cs:
- IBusinessDataService with all data functions from the spec
- Each function queries the tenant-scoped SplashSphereDbContext
- Returns structured DTOs that Claude can interpret
- Functions: GetRevenueSummary, GetProfitLoss, GetEmployeePerformance,
  GetTopServices, GetInventoryStatus, GetStockForecasts, 
  GetShiftVarianceHistory, GetCustomerRetention, GetCostPerWash,
  CompareBranches, GetPeakHourAnalysis, DetectAnomalies
- All functions accept tenantId (from TenantContext) + optional branchId + period

Application/Services/AiChatService.cs:
- Orchestrates: builds system prompt with tenant context, maps tool definitions,
  manages conversation history, calls AnthropicService, handles tool call loop,
  saves messages to database
- Respects query limits per plan
```

### Prompt 21.2 — AI Endpoints + Daily Brief + Alerts

```
Build:

Application/Features/Ai/:
- SendAiMessageCommand (conversationId?, message, branchId?)
  → orchestrates the full AI chat flow, returns streaming response
- GetConversationsQuery, GetConversationByIdQuery
- DeleteConversationCommand
- GetDailyBriefQuery (date?) → returns cached brief or generates on-demand
- GetSmartAlertsQuery → active, undismissed alerts
- DismissAlertCommand

API/Endpoints/AiEndpoints.cs:
- POST /ai/chat — streaming SSE response
- GET /ai/conversations — list with pagination
- GET /ai/conversations/{id}
- DELETE /ai/conversations/{id}
- GET /ai/daily-brief
- GET /ai/alerts
- PATCH /ai/alerts/{id}/dismiss
- GET /ai/usage

Feature gating: [RequiresFeature("negosyo_ai")]
Add "negosyo_ai" to Growth and Enterprise plan features in PlanCatalog.

Hangfire jobs:
- GenerateDailyBriefsJob: 6 AM PHT, for each Growth+ tenant
- DetectSmartAlertsJob: every 4 hours, runs anomaly detection
- CleanupConversationsJob: weekly, respects retention by plan
```

### Prompt 21.3 — AI Frontend (Admin Dashboard)

```
Build the Negosyo AI frontend in the admin app:

1. AI Chat — two options, let user choose:
   Option A: Floating button (bottom-right) → sliding sidebar panel (400px wide)
   Option B: /ai page with full-width chat

   Chat UI:
   - Message bubbles: user (right-aligned, splash bg) / AI (left, white bg)
   - Streaming text display (typewriter effect)
   - Tool call indicator: "🔍 Looking up your revenue data..."
   - Markdown rendering in AI responses (bold, bullets, tables)
   - Suggested question chips below AI responses
   - Conversation list sidebar (past conversations)
   - "New conversation" button

2. Daily Brief card on dashboard home page:
   - Above KPI cards, dismissible
   - Shows yesterday's key metrics + alerts + one insight
   - "Ask more" button opens AI chat pre-filled
   - "View full brief" expands to full daily report

3. Smart alert integration:
   - AI alerts appear in the notification bell dropdown with 🤖 icon
   - Each alert has a "Ask about this" button → opens AI chat with pre-filled question
   - Dismissible

4. AI usage indicator:
   - In settings or AI chat footer: "12 of 50 queries used today"
   - Growth plan: shows limit bar
   - Enterprise: shows "Unlimited"

5. Add "negosyo_ai" to FeatureGate checks:
   - Sidebar nav: "🤖 Negosyo AI" item (locked with lock icon on Starter)
   - Dashboard daily brief: hidden on Starter
   - Notification alerts: hidden on Starter
```

### Prompt 21.4 — Anomaly Detection + Smart Alert Engine

```
Build the smart alert detection engine:

Application/Services/AnomalyDetectionService.cs:
- DetectRevenueAnomalies: compare today vs 30-day rolling average
  Flag if >20% deviation. Include day-of-week adjustment.
- DetectCashVariancePatterns: find cashiers with 3+ consecutive 
  negative variances in their last 5 shifts.
- DetectEmployeePerformanceChanges: compare this week's commission 
  vs 4-week average per employee. Flag if >30% drop.
- DetectLowStockForecasts: for each supply, calculate days until 
  depletion at current usage rate. Alert if <5 days.
- DetectCustomerChurn: find Gold/Platinum customers with no 
  transaction in 30+ days.
- DetectPricingOpportunities: find services where margin is >10% 
  below the tenant's average margin.
- DetectPeakPatterns: analyze revenue by hour/day and suggest 
  promotional opportunities for low-traffic periods.
- DetectPayrollAnomalies: compare current payroll total vs 4-week avg.

Each detector returns a list of AiSmartAlert records.

The DetectSmartAlertsJob (every 4 hours):
1. For each Growth+ tenant, run all detectors
2. De-duplicate: don't create an alert if the same alert type 
   for the same entity already exists and is undismissed
3. For critical alerts (stock depletion < 2 days, cash variance 
   pattern), send SMS to the tenant owner if SMS is enabled
4. Save new alerts to the database
```

---

## Phase Summary

| Prompt | What | Layer |
|---|---|---|
| 21.1 | AI entities, Anthropic API integration, data functions, chat orchestration | Backend |
| 21.2 | AI endpoints (streaming), daily brief, alerts, Hangfire jobs, plan gating | Backend |
| 21.3 | AI chat UI (sidebar + page), daily brief card, alert integration | Frontend |
| 21.4 | Anomaly detection engine, smart alert detectors, SMS alerts | Backend |

**Total: 4 prompts in Phase 21.**

---

## Key Design Decisions

1. **Anthropic API with tool use, not fine-tuned models.** Philippine car wash data is too small and too specific for fine-tuning. Tool use lets Claude query the tenant's actual data at runtime. The system prompt gives it car wash domain knowledge. This approach requires zero ML infrastructure.

2. **Sonnet 4.6 for chat, not Opus.** Most questions ("how's my revenue?") don't need deep reasoning. Sonnet is 5x cheaper and 3x faster. Reserve Opus for the daily brief generation where deeper analysis is valuable.

3. **Data functions are tenant-scoped.** Every query uses `TenantContext.TenantId`. Claude never sees data from other tenants. The tool results are the only data Claude receives — it never has direct database access.

4. **Daily brief is pre-generated, not on-demand.** Generating a brief for 50 tenants at 6 AM costs ~$1 total and ensures instant display when the owner opens the dashboard. On-demand would add 3-5 seconds of latency.

5. **Smart alerts are batch-detected, not real-time.** Running anomaly detection every 4 hours is sufficient for business insights (these aren't time-critical like system alerts). This keeps API costs predictable.

6. **Conversation history is truncated.** Only the last 10 messages are sent to Claude on each request. This keeps token usage bounded while maintaining conversational context. Older messages are still stored for the owner to review.

7. **Filipino/Taglish support.** The system prompt tells Claude to respond in Filipino if the owner writes in Filipino. Most Philippine business owners communicate in Taglish (mixed Tagalog-English). Claude handles this naturally.

8. **Suggested questions are contextual.** After showing revenue data, suggest "Why did Saturday drop?" After showing employee data, suggest "Compare Juan and Pedro." This guides owners who don't know what to ask.

9. **Not a general-purpose chatbot.** The system prompt explicitly restricts Claude to car wash business topics. If someone asks "write me a poem," it redirects to business questions. This keeps the feature focused and prevents misuse of API credits.

10. **Negosyo AI is a Growth+ feature.** Starter tenants don't get AI — this is a premium differentiator that drives upgrades. "Upgrade to Growth to unlock your AI business advisor."
