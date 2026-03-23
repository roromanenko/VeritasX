# VeritasX Stats Feature — Codebase Analysis

**Date:** 2026-03-23
**Purpose:** Pre-implementation analysis to inform design of a bot statistics / P&L feature.

---

## 1. BotTradeRecord Structure

**Domain model** (`Core/Domain/BotTradeRecord.cs`):

| Field | Type | Notes |
|---|---|---|
| `Id` | `string` | Unique record ID |
| `BotId` | `string` | Bot that executed the trade |
| `UserId` | `string` | Owner of the bot |
| `Symbol` | `string` | Trading pair, e.g. `"BTCUSDT"` |
| `Side` | `OrderSide` enum | **Buy or Sell** — present |
| `Price` | `decimal` | Execution price — present |
| `Quantity` | `decimal` | Amount executed — present |
| `Reason` | `string` | Strategy signal reason |
| `TradeId` | `string?` | Optional link to underlying exchange `Trade` record |
| `ExecutedAt` | `DateTimeOffset` | Timestamp — present |

**MongoDB document** (`Infrastructure/Persistence/Entities/BotTradeRecordDocument.cs`) maps 1:1 to the domain model and is stored in the `bot_trade_records` collection.

**Key finding: trades are fully independent.**
There is no `PositionId`, no pairing between buy and sell records, and no concept of open/close position. The optional `TradeId` field links to the raw exchange `Trade` record, but it is not a pairing mechanism. Each `BotTradeRecord` represents a single atomic execution.

---

## 2. Existing Data Volume and Access Patterns

**`IBotTradeRepository` surface:**

| Method | Filter |
|---|---|
| `CreateTradeRecord` | — |
| `GetTradesByBotId(botId, limit = 100)` | Per-bot, sorted descending by `ExecutedAt` |
| `GetTradesByUserId(userId, limit = 100)` | Per-user, sorted descending by `ExecutedAt` |
| `GetTradesByDateRange(botId, from, to)` | Per-bot with date range |

All queries use simple `Find()` with equality/range filters, `.SortByDescending()`, and a configurable result limit. No aggregations exist anywhere in the codebase.

**Realistic trade volume:**

- Low-frequency strategy (e.g. daily rebalance): **1–5 trades/day per bot**
- Medium-frequency (e.g. 1h candle strategy): **10–50 trades/day per bot**
- A user running 5 bots: **50–250 trades/day** at peak
- After 1 year of operation a single bot could accumulate **~3,600–18,000 records** — manageable, not big-data scale

The default `limit = 100` cap in existing API endpoints means full-history queries are not currently supported at the API level.

---

## 3. P&L Calculation Feasibility

**Base currency:** `BotConfiguration.QuoteAsset` (e.g. `USDT`). P&L is denominated per-bot in its own quote asset.

**Challenge — no position pairs exist.**
Because trades are independent records, realized P&L cannot be read off a single record. It must be computed by matching buy-side cost against sell-side revenue. The two practical approaches:

| Approach | Feasibility with current schema |
|---|---|
| **FIFO matching** in application code | ✓ Possible — sort trades chronologically, accumulate buy quantities, pair against sells. No schema changes required but requires reading *all* historical trades for a bot, not just the last 100. |
| **Simple net cash flow** (sum of sell revenue minus sum of buy cost) | ✓ Possible — an approximation that works well for bots that maintain a single asset position. Easy to compute via aggregation. Less precise for bots that hold partial positions. |
| **Aggregation pipeline FIFO** | ✗ Not practical — FIFO requires stateful sequential processing that MongoDB pipelines cannot express natively. |

**For `RebalanceToTargetStrategy`** (the only current strategy), the bot buys and sells a basket of assets to maintain target weights. Net cash flow approximation is a good fit because the bot manages fractions of each asset continuously.

**Schema changes that would simplify things (optional future work):**
Adding a nullable `PositionId` and `ClosedPnl` field to `BotTradeRecord` would allow exact per-trade P&L attribution without post-processing, but this is not required for an MVP stats feature.

---

## 4. Multi-Exchange / Multi-Bot Considerations

**User → ExchangeConnection:**
`User.ExchangeConnections` is a `Dictionary<ExchangeName, ExchangeConnectionDocument>`. A user can have at most one connection per exchange (Binance, OKX, Bybit). API keys are stored AES-256-GCM encrypted.

**Bot → Exchange:**
`BotConfiguration.Exchange` (an `ExchangeName` enum value) identifies which exchange the bot uses. The bot retrieves the matching connection from its owner's dictionary at runtime.

**`BotTradeRecord` does NOT have an `Exchange` field.** Exchange is implicit — you derive it by looking at the bot's configuration. This means:

- A user's trades across multiple exchanges are only distinguishable at the bot level, not at the trade level.
- Cross-exchange aggregation requires joining `bot_trade_records` → `bot_configurations` → `exchange`.
- If multi-exchange P&L rollup is needed, an `Exchange` field should be denormalized onto `BotTradeRecord` for query efficiency.

---

## 5. Aggregation Feasibility in MongoDB

**Current state: zero aggregation pipelines are used anywhere in the codebase.**
Every repository uses `Find()` with filter/sort/limit. No `Aggregate()` calls exist.

**No architectural barrier exists** — `MongoDbContext.GetCollection<T>()` returns a standard `IMongoCollection<T>`, which supports the full aggregation API. Adding a pipeline would be a new pattern but not a breaking one.

**No index declarations** are present in any repository. Queries rely on MongoDB's default `_id` index and full collection scans for filter fields. For a statistics feature — which would need to sum across potentially thousands of trade records — a compound index on `(botId, executedAt)` would be essential before deploying aggregation pipelines.

---

## 6. Recommendation

### Chosen approach: **C — Hybrid (daily snapshots + live window)**

#### Rationale

**Why not A (pure on-the-fly aggregation)?**

- No aggregation pattern exists today; introducing it for the most complex query (FIFO P&L) would be a large first step with high maintenance cost.
- FIFO matching is stateful and cannot be expressed as a declarative pipeline — it would still require application-layer code for any precise P&L.
- The existing `limit = 100` pattern means full-history aggregation is actively discouraged. A bot with 12 months of history at medium frequency has ~18,000 records; scanning all of them on every dashboard load is wasteful.
- No indexes exist. Deploying aggregation without indexes would cause full collection scans at scale.

**Why not B (pure background snapshots)?**

- Pure snapshots would miss the most actionable metric: *what has happened in the current bot session or today so far*. Users want to see live P&L for running bots, not wait until midnight.
- Recency latency of one full day is a poor user experience for an active trader.

**Why C (hybrid) is correct:**

The hybrid approach maps cleanly onto two natural windows of data:

| Window | Mechanism | What it stores |
|---|---|---|
| **Historical (> 24h ago)** | Nightly background job | Pre-computed daily stats snapshot per bot |
| **Current session / today** | On-demand calculation | Live totals from recent trades (last 24h or since bot start) |

#### Proposed design

1. **New collection: `bot_daily_statistics`**
   Document shape:
   ```
   {
     botId, date (UTC day), quoteAsset,
     realizedPnl, totalBuyCost, totalSellRevenue,
     tradeCount, buyCount, sellCount,
     computedAt
   }
   ```

2. **Background job: `BotStatisticsJob`** (modelled on the existing `DatabaseCleanupJob`)
   - Runs nightly (e.g. 00:05 UTC)
   - For each bot, queries `GetTradesByDateRange` for the previous day
   - Computes net cash-flow P&L (sell revenue − buy cost) per day
   - Upserts a `bot_daily_statistics` document

3. **Live endpoint: `GET /api/bots/{id}/statistics?from=&to=`**
   - For date ranges entirely in the past: serve from pre-computed snapshots
   - For ranges including today: fetch snapshots for past days + live calculation for today's trades

4. **Indexes to add (required before shipping):**
   - `bot_trade_records`: compound `(botId, executedAt)`
   - `bot_daily_statistics`: compound `(botId, date)` with unique constraint

#### Risks and mitigations

| Risk | Mitigation |
|---|---|
| Bot runs across midnight — day boundary split | Aggregate by UTC day consistently; acceptable approximation |
| Bot has no QuoteAsset? | `BotConfiguration.QuoteAsset` is required; validate at bot creation |
| Snapshot job fails for one bot | Log error, skip that bot; job is idempotent (upsert), will self-heal next night |
| Exchange field missing from trades | Derive from BotConfiguration at snapshot compute time; denormalize into snapshot |

---

### Summary

| Question | Finding |
|---|---|
| Trade structure | Independent records; Side, Price, Quantity, Timestamp all present; no position pairs |
| P&L calculation | Possible via net cash-flow; FIFO requires reading full history; no schema change strictly needed for MVP |
| Repository queries | Simple Find only; no aggregations; limit-capped |
| Multi-exchange | Exchange implicit via bot config; not on trade record |
| MongoDB aggregation | Fully supported by driver; zero usage today; needs indexes first |
| **Recommendation** | **Hybrid: nightly snapshots + live 24h window** |
