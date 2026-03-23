# VeritasX Refactoring Plan

**Scope:** Pre-P&L statistics refactoring — establishes foundations for Strategy DSL Editor,
Strategy Marketplace, and per-bot P&L statistics.

**Analysis date:** 2026-03-23
**Analyst:** Claude (architecture analysis only — no implementation code was written)

---

## 1. Current State Summary

### 1.1 Trade (`Core/Domain/Trade.cs`)

```
Trade
├── Id                : string (required)
├── UserId            : string? (nullable)
├── Exchange          : ExchangeName (required)
├── ExchangeOrderId   : string (required)
├── ExchangeTradeId   : string (required)
├── IsTestnet         : bool
├── Symbol            : string (required)
├── Side              : OrderSide
├── Price             : decimal
├── Quantity          : decimal
├── QuoteQuantity     : decimal
├── Fee               : decimal
├── FeeAsset          : string
└── ExecutedAt        : DateTimeOffset
```

**Missing per the decisions:** `TradeSource` enum field, nullable `BotId`.

### 1.2 TradeDocument (`Infrastructure/Persistence/Entities/TradeDocument.cs`)

Mirrors `Trade` exactly. Collection name: `trades` (via `[Table("trades")]`).
**Missing:** `TradeSource` (string/enum), `BotId` (ObjectId?).

### 1.3 BotTradeRecord (`Core/Domain/BotTradeRecord.cs`)

```
BotTradeRecord
├── Id           : string (required)
├── BotId        : string (required)
├── UserId       : string (required)
├── Symbol       : string (required)
├── Side         : OrderSide (required)
├── Price        : decimal (required)
├── Quantity     : decimal (required)
├── Reason       : string (required)
├── TradeId      : string? (nullable link back to Trade.Id)
└── ExecutedAt   : DateTimeOffset
```

**Missing per the decisions:** `Exchange` (ExchangeName — denormalized).

### 1.4 BotTradeRecordDocument (`Infrastructure/Persistence/Entities/BotTradeRecordDocument.cs`)

Collection name: `bot_trade_records` (via `[Table("bot_trade_records")]`).
Fields mirror the domain model using `ObjectId` for Id/BotId/UserId/TradeId.
**Missing:** `Exchange` field (string).

### 1.5 BotConfiguration (`Core/Domain/BotConfiguration.cs`)

```
BotConfiguration
├── Id                   : string (required)
├── UserId               : string (required)
├── Name                 : string
├── Exchange             : ExchangeName (required, init-only)
├── Symbol               : string (required, init-only)
├── BaseAsset            : string (required, init-only)
├── QuoteAsset           : string (required, init-only)
├── Strategy             : StrategyDefinition (required, init-only)  ← target for replacement
├── RiskParameters       : RiskParameters
├── Status               : BotStatus
├── ErrorMessage         : string?
├── MaxConsecutiveErrors : int (default 5)
├── CreatedAt            : DateTimeOffset
├── StartedAt            : DateTimeOffset?
└── StoppedAt            : DateTimeOffset?

StrategyDefinition (nested, defined in same file)
├── Type       : StrategyType (required, init-only)
└── Parameters : Dictionary<string, string> (required, init-only)

RiskParameters (nested, defined in same file)
├── PositionSize : decimal
├── StopLoss     : decimal?
└── TakeProfit   : decimal?
```

**Target for replacement:** `Strategy: StrategyDefinition` → `StrategyId`, `StrategySnapshot`, `StrategyVersion`.

### 1.6 BotConfigurationDocument (`Infrastructure/Persistence/Entities/BotConfigurationDocument.cs`)

Collection: `bot_configurations`.
Contains embedded `StrategyDefinitionDocument` and `RiskParametersDocument` sub-documents.

### 1.7 StrategyType (`Core/Domain/StrategyType.cs`)

```csharp
public enum StrategyType { DeltaRebalancing }
```

Single value today. Referenced by `StrategyDefinition`, `StrategyDefinitionDocument`,
`StrategyDefinitionDto`, `StrategyFactory`, and `BotRunnerTests`.

### 1.8 IStrategyFactory / StrategyFactory

```
Trading/Strategies/IStrategyFactory.cs   — ITradingStrategy Create(StrategyDefinition definition)
Trading/Strategies/StrategyFactory.cs    — switch on StrategyType.DeltaRebalancing → new RebalanceToTargetStrategy(...)
```

`StrategyFactory` deserializes `definition.Parameters` as JSON into `RebalanceConfig`.

### 1.9 ITradingStrategy (`Trading/Strategies/ITradingStrategy.cs`)

```
ITradingStrategy
├── DataRequirement : DataRequirement { Ticker | Kline }
└── Task<TradingSolution> CalculateNextStep(TradingContext, MarketTick, CancellationToken)
```

### 1.10 ITradeExecutor (`Core/Interfaces/ITradeExecutor.cs`)

```
Task<BotTradeRecord?> ExecuteAsync(TradingSolution solution, BotConfiguration bot, CancellationToken)
```

Implemented in `Infrastructure/Trading/TradeExecutor.cs`. Creates both a `TradeDocument`
(persisted to `trades`) and returns a `BotTradeRecord` (caller persists to `bot_trade_records`).

### 1.11 BotRunner end-to-end strategy wiring

**Construction path:**
1. `BotManagerBackgroundService.StartPendingBotsAsync()` maps a `BotConfigurationDocument` → `BotConfiguration` via AutoMapper.
2. Calls `IBotRunnerFactory.Create(bot)` → `BotRunnerFactory.Create(bot)`.
3. `BotRunnerFactory` uses `ActivatorUtilities.CreateInstance<BotRunner>(scope.ServiceProvider, scope, bot)` — resolves `IStrategyFactory` from DI.
4. `BotRunner.StartAsync()` calls `_strategyFactory.Create(_bot.Strategy)` at line 75 — strategy is materialized at start, not at construction.
5. `_bot.Strategy.Type` is also accessed at line 85 (log) and line 102 (`GetKlineInterval` reads `strategy.Parameters["interval"]`).

**Key observation:** `_bot` (a `BotConfiguration`) is captured for the entire lifetime of the runner.
The `StrategyDefinition` is only ever used as input to `IStrategyFactory.Create()` and for the
`GetKlineInterval` helper.

### 1.12 Existing index declarations

**None found.** Grep for `CreateIndex`, `EnsureIndex`, `IndexKeys`, `Indexes.` returns zero matches
across the entire solution. There is no `MongoIndexInitializer` or equivalent today.

### 1.13 Bot update validation — current state

The only status guard lives in the **controller** at:

```
Api/Controllers/BotsController.cs  line 182
    if (bot.Status is BotStatus.Active or BotStatus.Pending)
        return Ok(new ApiResponse<BotDto>(false, "Bot must be stopped before editing"));
```

`BotService.UpdateBot()` performs no status check at all — it maps and passes straight to the repository.
`IBotService.UpdateBot()` signature: `Task UpdateBot(BotConfiguration bot)` — no guard at the service level.

---

## 2. New Domain Models (Proposed C# Shapes)

### 2.1 TradeSource enum

Add to `Core/Domain/Trade.cs` (or a new `Core/Domain/TradeSource.cs`):

```csharp
public enum TradeSource
{
    Manual,
    Bot,
    Api
}
```

### 2.2 Updated Trade

```csharp
public sealed class Trade
{
    // ... existing fields unchanged ...
    public TradeSource Source { get; set; } = TradeSource.Manual;
    public string? BotId { get; set; }
}
```

### 2.3 Updated BotTradeRecord

```csharp
public class BotTradeRecord
{
    // ... existing fields unchanged ...
    public required ExchangeName Exchange { get; init; }  // denormalized
}
```

### 2.4 Strategy (marketplace entry) — new domain model

Proposed location: `Core/Domain/Strategy.cs`

```csharp
public class Strategy
{
    public required string Id { get; init; }
    public required string AuthorId { get; init; }
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsVeritasOfficial { get; set; }
    public bool IsPublic { get; set; }
    public decimal? Price { get; set; }          // null = free
    public string? DslText { get; set; }         // text-based DSL
    public string? GraphJson { get; set; }       // Blockly visual editor graph
    public int Version { get; set; } = 1;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
```

### 2.5 UserStrategyLibrary — new domain model

Proposed location: `Core/Domain/UserStrategyLibrary.cs`

```csharp
public class UserStrategyLibrary
{
    public required string Id { get; init; }
    public required string UserId { get; init; }
    public required string StrategyId { get; init; }     // reference to Strategy.Id
    public required string SavedDslSnapshot { get; init; } // DSL copied at save time
    public string? SavedGraphSnapshot { get; init; }
    public required int SavedVersion { get; init; }
    public DateTimeOffset SavedAt { get; init; } = DateTimeOffset.UtcNow;
}
```

### 2.6 Updated BotConfiguration — strategy fields

Replace `StrategyDefinition Strategy` with three fields:

```csharp
public class BotConfiguration
{
    // ... existing fields (Id, UserId, Name, Exchange, Symbol, etc.) unchanged ...

    // NEW — replaces: public required StrategyDefinition Strategy { get; init; }
    public required string StrategyId { get; init; }          // reference to UserStrategyLibrary.Id
    public required string StrategySnapshot { get; set; }     // DSL/JSON copied at bot start
    public int StrategyVersion { get; set; }                  // version at time of snapshot

    public required RiskParameters RiskParameters { get; set; }
    // ... remaining fields unchanged ...
}
```

**Note:** `StrategyDefinition` and `StrategyType` can be removed from `Core/Domain/BotConfiguration.cs`
once `BotConfiguration` is migrated. They are currently defined in the same file as `BotConfiguration`.

### 2.7 MongoDB document shapes

**StrategyDocument** (collection `strategies`):

```csharp
[Table("strategies")]
public class StrategyDocument
{
    [BsonId] public ObjectId Id { get; set; }
    public ObjectId AuthorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsVeritasOfficial { get; set; }
    public bool IsPublic { get; set; }
    [BsonIgnoreIfNull] public decimal? Price { get; set; }
    [BsonIgnoreIfNull] public string? DslText { get; set; }
    [BsonIgnoreIfNull] public string? GraphJson { get; set; }
    public int Version { get; set; } = 1;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
```

**UserStrategyLibraryDocument** (collection `user_strategy_library`):

```csharp
[Table("user_strategy_library")]
public class UserStrategyLibraryDocument
{
    [BsonId] public ObjectId Id { get; set; }
    public ObjectId UserId { get; set; }
    public ObjectId StrategyId { get; set; }
    public string SavedDslSnapshot { get; set; } = string.Empty;
    [BsonIgnoreIfNull] public string? SavedGraphSnapshot { get; set; }
    public int SavedVersion { get; set; }
    public DateTimeOffset SavedAt { get; set; }
}
```

---

## 3. File Change Inventory

### 3.1 Core layer (`Core/`)

| File | Change required |
|---|---|
| `Core/Domain/Trade.cs` | Add `TradeSource Source` + `string? BotId` |
| `Core/Domain/BotConfiguration.cs` | Replace `StrategyDefinition Strategy` with `StrategyId`, `StrategySnapshot`, `StrategyVersion`; remove `StrategyDefinition` class; remove `StrategyType` enum (it moves or is deleted) |
| `Core/Domain/StrategyType.cs` | Delete (enum only used to drive `IStrategyFactory`; superseded by DSL) |
| `Core/Domain/Strategy.cs` | **New file** — `Strategy` domain model |
| `Core/Domain/UserStrategyLibrary.cs` | **New file** — `UserStrategyLibrary` domain model |
| `Core/Interfaces/IBotService.cs` | No change required beyond what service layer dictates |

### 3.2 Infrastructure — persistence entities (`Infrastructure/Persistence/Entities/`)

| File | Change required |
|---|---|
| `TradeDocument.cs` | Add `string? Source`, `ObjectId? BotId` |
| `BotTradeRecordDocument.cs` | Add `string Exchange` |
| `BotConfigurationDocument.cs` | Replace `StrategyDefinitionDocument Strategy` with `string StrategyId`, `string StrategySnapshot`, `int StrategyVersion`; remove `StrategyDefinitionDocument` class |
| `StrategyDocument.cs` | **New file** |
| `UserStrategyLibraryDocument.cs` | **New file** |

### 3.3 Infrastructure — repositories (`Infrastructure/Persistence/Repositories/` + `Infrastructure/Interfaces/`)

| File | Change required |
|---|---|
| `Infrastructure/Interfaces/IStrategyRepository.cs` | **New file** — CRUD for `StrategyDocument` |
| `Infrastructure/Persistence/Repositories/StrategyRepository.cs` | **New file** |
| `Infrastructure/Interfaces/IUserStrategyLibraryRepository.cs` | **New file** — CRUD for `UserStrategyLibraryDocument` |
| `Infrastructure/Persistence/Repositories/UserStrategyLibraryRepository.cs` | **New file** |
| `BotTradeRepository.cs` | No interface change; implementation unchanged |
| `TradeRepository.cs` | No interface change; internal `TradeDocument` fields change transparently |

### 3.4 Infrastructure — mapping (`Infrastructure/Mapping/Profiles/`)

| File | Change required |
|---|---|
| `BotProfile.cs` | Remove `CreateMap<StrategyDefinition, StrategyDefinitionDocument>().ReverseMap()` — replace with new `StrategyId`/`StrategySnapshot`/`StrategyVersion` flat mappings; update `BotTradeRecord ↔ BotTradeRecordDocument` maps to include `Exchange` |
| `TradeProfile.cs` | Add mapping for `Source` (`TradeSource ↔ string`) and `BotId` (`string? ↔ ObjectId?`) |
| `StrategyProfile.cs` | **New file** — `Strategy ↔ StrategyDocument`, `UserStrategyLibrary ↔ UserStrategyLibraryDocument` |

### 3.5 Infrastructure — services and trading

| File | Change required |
|---|---|
| `Infrastructure/Services/BotService.cs` | `CreateBot()` must set `StrategySnapshot` from the library at creation time (or delegate to a new `StrategySnapshotService`); `UpdateBot()` must enforce status guard (see §4) |
| `Infrastructure/Trading/BotRunner.cs` | `StartAsync()`: replace `_strategyFactory.Create(_bot.Strategy)` with DSL-interpreter instantiation from `_bot.StrategySnapshot`; remove `_strategyFactory` field and constructor parameter; remove `GetKlineInterval(StrategyDefinition)` helper (or re-derive from snapshot); log line 85 referencing `_bot.Strategy.Type` must change |
| `Infrastructure/Trading/BotRunnerFactory.cs` | No structural change; `IStrategyFactory` removal cleans up one resolved dependency |
| `Infrastructure/Trading/TradeExecutor.cs` | `ExecuteAsync()`: when creating `TradeDocument`, set `Source = TradeSource.Bot` and `BotId = ObjectId.Parse(bot.Id)`; when returning `BotTradeRecord`, populate `Exchange = bot.Exchange` |
| `Infrastructure/Jobs/BotManagerBackgroundService.cs` | No changes required for the refactoring itself (validation enforcement is at service layer, not here) |

### 3.6 Trading layer (`Trading/`)

| File | Change required |
|---|---|
| `Trading/Strategies/IStrategyFactory.cs` | **Delete** (once BotRunner is migrated to DSL; keep until DSL interpreter is ready — see §8) |
| `Trading/Strategies/StrategyFactory.cs` | **Delete** (same condition) |
| `Trading/Strategies/RebalanceToTargetStrategy.cs` | **Keep** as a built-in; will be invoked via DSL interpreter, not via factory |
| `Trading/Strategies/ITradingStrategy.cs` | Keep; all strategies implement this |
| `Trading/TradingContext.cs` | No change |

### 3.7 Api layer — DTOs and mappings (`Api/`)

| File | Change required |
|---|---|
| `Api/DTO/BotRequests.cs` | `CreateBotRequest`: replace `StrategyDefinitionDto Strategy` with `string StrategyId` (user picks from library); `UpdateBotRequest`: remove `StrategyParameters` dict (strategy no longer editable by key-value); strategy changes by replacing `StrategyId` only (but only when stopped) |
| `Api/DTO/BotResponses.cs` | `BotDto`: replace `StrategyDefinitionDto Strategy` with `string StrategyId`, `string StrategySnapshot`, `int StrategyVersion`; `BotTradeRecordDto`: add `string Exchange` |
| `Api/Mapping/BotDtoProfile.cs` | Remove `CreateMap<StrategyDefinition, StrategyDefinitionDto>().ReverseMap()`; update `CreateBotRequest → BotConfiguration` mapping; update `BotTradeRecord → BotTradeRecordDto` to include `Exchange` |
| `Api/Extensions/ServiceCollectionExtensions.cs` | Register new repositories; register `MongoIndexInitializer`; remove `IStrategyFactory` / `StrategyFactory` registration when factory is deleted |

### 3.8 Tests

| File | Change required |
|---|---|
| `Infrastructure.Tests/TradingTests/BotRunnerTests.cs` | Remove `Mock<IStrategyFactory>` and `Mock<ITradingStrategy>`; replace with mock DSL executor or a concrete `RebalanceToTargetStrategy` wired via snapshot; update `BotConfiguration` fixture to use new strategy fields |

---

## 4. Bot Update Validation — Proposed Approach

### Current state

The Active/Pending guard exists **only** at the controller level (`BotsController.EditBot`, lines 182–183).
`BotService.UpdateBot()` has no guard. Any caller that bypasses the controller (internal services,
future background jobs) can mutate an Active bot today.

### Recommendation: enforce at service layer

Add the guard to `BotService.UpdateBot()` (or a new `BotService.EditBotConfig()` method):

```
// in BotService.UpdateBot(BotConfiguration bot):
if (bot.Status is BotStatus.Active or BotStatus.Pending)
    throw new InvalidOperationException($"Bot '{bot.Id}' cannot be modified while active or pending.");
```

**Rationale:**
- The service layer is the correct enforcement boundary — it protects all callers, not just HTTP.
- The controller check can remain as a user-friendly early return (avoiding a 500 for a known bad state),
  but the service is the source of truth.
- The domain model (`BotConfiguration`) is too thin to self-enforce this rule (it has no knowledge of
  running state beyond a status property), and adding behavior to a plain data class would violate the
  existing architecture pattern.

**Do not** enforce this in `BotManagerBackgroundService` — that service does not call `UpdateBot()` on
behalf of user requests; it only calls it to set status transitions (Active/Stopped/Error), which must
be permitted at any time.

### Scope of the guard

The guard must cover **all** mutable parameters:
- `Name`
- `MaxConsecutiveErrors`
- `RiskParameters` (PositionSize, StopLoss, TakeProfit)
- `StrategyId` / `StrategySnapshot` / `StrategyVersion` (after refactoring)

It must **not** block status-only writes (the `SetStatusAsync` helper inside `BotRunner` calls
`IBotRepository.UpdateBot` directly — this is a document-level update, not `BotService.UpdateBot`,
so it is unaffected).

---

## 5. Index Strategy

### 5.1 Current state

Zero index declarations exist anywhere in the codebase. All queries run without any explicit index.
For small datasets this is fine; for P&L statistics queries (aggregations over thousands of trades
per bot per day) it is not.

### 5.2 Proposed: MongoIndexInitializer

Create a new class `Infrastructure/Persistence/MongoDb/MongoIndexInitializer.cs`:

```
MongoIndexInitializer
└── InitializeAsync(CancellationToken) : Task
    Calls IMongoDbContext.GetCollection<T>() and creates indexes declaratively.
```

Register it as a **hosted service** (or call it from `AddMongoDbServices` in
`ServiceCollectionExtensions.cs` via `IHostedService`). Running it at startup is idempotent
— `CreateIndexes` with the same key pattern is a no-op if the index already exists.

### 5.3 Required indexes

| Collection | Index | Type | Notes |
|---|---|---|---|
| `bot_trade_records` | `(botId, executedAt)` | Compound | Required before P&L stats; supports date-range queries per bot |
| `user_strategy_library` | `(userId, strategyId)` | Compound | Supports "find user's saved copy of strategy X" |
| `bot_daily_statistics` | `(botId, date)` | Compound unique | Future collection; add when collection is created |

### 5.4 Where index creation lives

`Api/Extensions/ServiceCollectionExtensions.cs` → `AddMongoDbServices()`:

```csharp
services.AddHostedService<MongoIndexInitializer>();
```

`MongoIndexInitializer` implements `IHostedService` and runs `InitializeAsync` in `StartAsync`.
This keeps all database wiring centralized in `ServiceCollectionExtensions` without spreading
index creation into individual repositories.

---

## 6. BotRunner / BotManagerBackgroundService Impact

### 6.1 How BotRunner currently receives its strategy

1. `BotRunner` is constructed with `IStrategyFactory _strategyFactory` (injected from DI scope via
   `ActivatorUtilities.CreateInstance`).
2. `StartAsync()` calls `_strategyFactory.Create(_bot.Strategy)` — strategy is resolved **once** at
   start, then used for every tick.
3. `_bot.Strategy` is also accessed directly in:
   - Line 85: `_bot.Strategy.Type` (log message)
   - Line 102: `GetKlineInterval(_bot.Strategy)` reads `strategy.Parameters["interval"]`

### 6.2 What changes in BotRunner after refactoring

`_bot.Strategy` no longer exists. Instead `_bot.StrategySnapshot` (a DSL string or graph JSON) is available.

**Required changes in `StartAsync()`:**

| Current | After refactoring |
|---|---|
| `var strategy = _strategyFactory.Create(_bot.Strategy);` | `var strategy = _dslInterpreter.Build(_bot.StrategySnapshot);` |
| `_logger.LogInformation(..., _bot.Strategy.Type, ...)` | Log strategy name/id from snapshot or from `_bot.StrategyId` |
| `GetKlineInterval(_bot.Strategy)` helper reads `Parameters["interval"]` | Interval must be derivable from the snapshot (DSL interpreter must expose `DataRequirement` and any interval metadata) |

**Constructor changes:**
- Remove `IStrategyFactory strategyFactory` parameter.
- Add `IDslStrategyInterpreter dslInterpreter` parameter (new interface, resolves `ITradingStrategy` from a snapshot string).
- `GetKlineInterval(StrategyDefinition)` is replaced by an equivalent that reads from the DSL interpreter's metadata.

**`BotRunnerFactory.cs`:** No structural change; `IStrategyFactory` is no longer resolved from the scope.

**`ServiceCollectionExtensions.cs`:** Remove `services.AddScoped<IStrategyFactory, StrategyFactory>()`.
Register `IDslStrategyInterpreter` instead.

### 6.3 Snapshot-at-start pattern

The decision that `StrategySnapshot` is **copied at bot start** (not read live from the library) means
`BotRunner.StartAsync()` can simply pass `_bot.StrategySnapshot` to the interpreter. The snapshot is
already baked into `_bot` by the time the runner is created — `BotManagerBackgroundService` does not
need to fetch anything extra.

**However**, the copy-at-start logic must happen somewhere. Two options:

**Option A (recommended):** `BotService.StartBot()` copies the snapshot from `UserStrategyLibrary`
into `BotConfiguration.StrategySnapshot` before setting status to `Pending`.

**Option B:** `BotRunner.StartAsync()` fetches the snapshot from `IUserStrategyLibraryRepository`
using `_bot.StrategyId`. This adds a repository dependency to `BotRunner` and a DB call at start.

Option A is cleaner — the snapshot is frozen in the document before the bot is picked up by the manager.

### 6.4 BotManagerBackgroundService impact

`BotManagerBackgroundService` **does not need changes** for:
- The "no updates while active" rule — enforcement is at service layer, not here.
- The snapshot copy — happens in `BotService.StartBot()` before status is set to Pending.

`BotManagerBackgroundService` **will need a trivial change** if/when `IStrategyFactory` is removed
from DI — but since it doesn't reference `IStrategyFactory` directly, there is no impact.

---

## 7. Risk Register

### R1 — StrategyDefinition removal breaks BotDtoProfile and the frontend API client

**Severity: High**
`BotDto` currently includes `StrategyDefinitionDto Strategy` which renders the strategy type and
parameters dict in the UI (`BotDetail.tsx` likely uses these fields). When `StrategyDefinitionDto`
is removed, the generated TypeScript client (`tv-chart/src/api/`) will change — any frontend code
accessing `bot.strategy.type` or `bot.strategy.parameters` will break at compile time.

**Mitigation:** Audit `tv-chart/src/` for all usages of `bot.strategy` before removing the DTO.
Coordinate frontend changes with the API client regeneration step.

### R2 — BotRunner test suite has deep IStrategyFactory coupling

**Severity: Medium**
`Infrastructure.Tests/TradingTests/BotRunnerTests.cs` mocks both `IStrategyFactory` and
`ITradingStrategy`. When `IStrategyFactory` is removed from `BotRunner`'s constructor, all existing
tests will fail to compile. The test fixture also constructs `BotConfiguration` with a `StrategyDefinition`
(which will no longer exist).

**Mitigation:** Plan a test-rewrite pass as part of the BotRunner migration step (step 6 in §8).
The new tests should mock `IDslStrategyInterpreter` instead.

### R3 — BotRunner references `_bot.Strategy` in three places

**Severity: Medium**
Lines 75, 85, 102 in `BotRunner.cs`. Line 85 is a log message (low risk). Line 102
(`GetKlineInterval`) is a private helper that reads `strategy.Parameters["interval"]` — this logic
must be re-implemented against the DSL snapshot format, and the correct format is not yet defined.
If the DSL does not expose interval metadata, the `DataRequirement.Kline` path in `BotRunner` may
need a separate mechanism.

**Mitigation:** Define the minimal DSL/snapshot metadata contract (at minimum: `DataRequirement`
and optional `Interval`) before migrating `BotRunner`.

### R4 — TradeExecutor creates TradeDocument directly (no mapper)

**Severity: Low**
`TradeExecutor.ExecuteAsync()` constructs a `TradeDocument` via object initializer (lines 91–103),
bypassing AutoMapper. Adding `Source` and `BotId` to `TradeDocument` requires a manual change here,
not just a mapper update. This is easy to miss if the change plan assumes "just update the mapper."

**Mitigation:** Note explicitly: `TradeExecutor.cs` line 91 is an independent write path for
`TradeDocument` that must be updated separately from `TradeProfile.cs`.

### R5 — BotService.UpdateBot uses full document replace

**Severity: Low**
`BotRepository.UpdateBot()` uses `ReplaceOneAsync` — it overwrites the entire document. If
`BotConfiguration` no longer contains `StrategySnapshot` at the point of an update call (e.g., if
the domain model is partially migrated), the snapshot will be zeroed out. Migrations of domain
model and repository must be atomic within a step.

### R6 — No migration needed, but stale running bots at refactoring time

**Severity: Low (dev environment only)**
The decision states no data migration is needed (not in production). However, if any bot is in
`Active` or `Pending` state in the dev MongoDB when the refactoring is deployed, the deserialization
of `BotConfigurationDocument` will fail if the schema changes break BSON mapping. Stop all bots
and clear the collection before deploying schema changes in dev.

### R7 — StrategyType enum is used by the TypeScript generated client

**Severity: Low**
`StrategyType` is exposed in `StrategyDefinitionDto` which is part of the API surface. The generated
TypeScript client will have a `StrategyType` enum. Removing it requires a `generate-api` run and
any frontend code referencing `StrategyType.DeltaRebalancing` will break.

**Mitigation:** Same as R1 — coordinate frontend audit with API surface changes.

### R8 — IStrategyFactory is scoped; BotRunner resolves it from a DI scope

**Severity: Informational**
`IStrategyFactory` is registered as `AddScoped` (line 154, `ServiceCollectionExtensions.cs`).
`BotRunner` gets a fresh scope via `BotRunnerFactory`. When `IDslStrategyInterpreter` is added,
confirm its lifetime matches — if it holds any state (e.g., compiled strategy cache), it should be
singleton or transient, not scoped.

### R9 — `IBotService.UpdateBot` is called by BotRunner indirectly (via repository, not service)

**Severity: Informational**
`BotRunner.SetStatusAsync()` calls `_botRepository.UpdateBot(doc)` directly (bypassing
`IBotService`). Adding a status guard to `IBotService.UpdateBot()` will not block `BotRunner`'s
status updates, which is the correct behaviour. This is noted here so the implementer does not
accidentally route `SetStatusAsync` through the service layer.

---

## 8. Recommended Implementation Sequence

Steps are ordered to ensure the build never breaks at the end of each step. Steps within a group
can be done in parallel.

### Step 1 — Trade model additions (low risk, fully additive)

**Files:** `Core/Domain/Trade.cs`, `Infrastructure/Persistence/Entities/TradeDocument.cs`,
`Infrastructure/Mapping/Profiles/TradeProfile.cs`, `Infrastructure/Trading/TradeExecutor.cs`

1. Add `TradeSource` enum to `Core/Domain/Trade.cs` (or its own file).
2. Add `TradeSource Source` and `string? BotId` to `Trade`.
3. Add `string? Source` and `ObjectId? BotId` to `TradeDocument`.
4. Update `TradeProfile.cs` to map the new fields.
5. Update `TradeExecutor.ExecuteAsync()` to set `Source = TradeSource.Bot` and `BotId`.

These are purely additive — no existing code breaks.

### Step 2 — BotTradeRecord Exchange field (low risk, fully additive)

**Files:** `Core/Domain/BotTradeRecord.cs`, `Infrastructure/Persistence/Entities/BotTradeRecordDocument.cs`,
`Infrastructure/Mapping/Profiles/BotProfile.cs`, `Api/DTO/BotResponses.cs`,
`Api/Mapping/BotDtoProfile.cs`, `Infrastructure/Trading/TradeExecutor.cs`

1. Add `ExchangeName Exchange` to `BotTradeRecord`.
2. Add `string Exchange` to `BotTradeRecordDocument`.
3. Update `BotProfile.cs` maps for `BotTradeRecord ↔ BotTradeRecordDocument`.
4. Add `string Exchange` to `BotTradeRecordDto`.
5. Update `BotDtoProfile.cs` for `BotTradeRecord → BotTradeRecordDto`.
6. Update `TradeExecutor.ExecuteAsync()` to populate `Exchange = bot.Exchange` in the returned `BotTradeRecord`.

### Step 3 — New domain models and MongoDB documents (additive, no existing code touched)

**Files:** New only — `Core/Domain/Strategy.cs`, `Core/Domain/UserStrategyLibrary.cs`,
`Infrastructure/Persistence/Entities/StrategyDocument.cs`,
`Infrastructure/Persistence/Entities/UserStrategyLibraryDocument.cs`

Create the new domain models and their documents. No interfaces, no registration yet.

### Step 4 — New repositories and DI registration

**Files:** `Infrastructure/Interfaces/IStrategyRepository.cs`,
`Infrastructure/Persistence/Repositories/StrategyRepository.cs`,
`Infrastructure/Interfaces/IUserStrategyLibraryRepository.cs`,
`Infrastructure/Persistence/Repositories/UserStrategyLibraryRepository.cs`,
`Infrastructure/Mapping/Profiles/StrategyProfile.cs`,
`Api/Extensions/ServiceCollectionExtensions.cs`

1. Implement the two new repositories.
2. Add `StrategyProfile.cs` AutoMapper profile.
3. Register in `ServiceCollectionExtensions.AddMongoDbServices()`.
4. Register `StrategyProfile` in `AddApplicationServices()` AutoMapper config.

### Step 5 — MongoIndexInitializer

**Files:** `Infrastructure/Persistence/MongoDb/MongoIndexInitializer.cs`,
`Api/Extensions/ServiceCollectionExtensions.cs`

1. Implement `MongoIndexInitializer` as a hosted service.
2. Create indexes for `bot_trade_records (botId, executedAt)` and `user_strategy_library (userId, strategyId)`.
3. Register it in `AddMongoDbServices()`.

This step is safe at any point after Step 4.

### Step 6 — BotConfiguration strategy field migration

**This is the highest-risk step. Do it atomically.**

**Files:** `Core/Domain/BotConfiguration.cs` (remove `StrategyDefinition`, `StrategyType`),
`Core/Domain/StrategyType.cs` (delete), `Infrastructure/Persistence/Entities/BotConfigurationDocument.cs`
(remove `StrategyDefinitionDocument`), `Infrastructure/Mapping/Profiles/BotProfile.cs`,
`Api/DTO/BotRequests.cs`, `Api/DTO/BotResponses.cs`, `Api/Mapping/BotDtoProfile.cs`,
`Infrastructure/Services/BotService.cs` (update `CreateBot`, `StartBot`),
`Infrastructure/Trading/BotRunner.cs` (replace strategy factory with DSL interpreter),
`Infrastructure.Tests/TradingTests/BotRunnerTests.cs` (rewrite affected tests)

**Pre-condition:** `IDslStrategyInterpreter` interface must be defined and at least a stub
implementation (that wraps `RebalanceToTargetStrategy`) must exist before this step.

Order within this step:
1. Define `IDslStrategyInterpreter` interface in `Core/Interfaces/`.
2. Implement a stub `DslStrategyInterpreter` in `Trading/` that handles the built-in
   `RebalanceToTargetStrategy` from the snapshot JSON.
3. Register it in `ServiceCollectionExtensions`.
4. Migrate `BotConfiguration` fields and all downstream files together.
5. Update `BotService.StartBot()` to copy the snapshot from `UserStrategyLibrary`.
6. Rewrite `BotRunnerTests`.

### Step 7 — Bot update validation at service layer

**Files:** `Infrastructure/Services/BotService.cs`

Add the Active/Pending guard to `BotService.UpdateBot()`. This is safe to do independently of Step 6
but is logically part of the same cleanup.

### Step 8 — Remove IStrategyFactory (cleanup, after Step 6 is verified)

**Files:** `Trading/Strategies/IStrategyFactory.cs` (delete), `Trading/Strategies/StrategyFactory.cs`
(delete), `Api/Extensions/ServiceCollectionExtensions.cs` (remove registration)

Only proceed after Step 6 is merged and stable. Removing these files while `BotRunner` still
references them will fail the build.

### Step 9 — Frontend audit and API client regeneration

**Files:** `tv-chart/src/` (audit), then `npm run generate-api`

After Steps 6–8 change the API surface, regenerate the TypeScript client and fix all frontend
compilation errors. Specifically audit:
- All usages of `bot.strategy` / `BotDto.strategy`
- Any reference to `StrategyType` enum in TypeScript
- `BotDetail.tsx` edit form (currently sends `StrategyParameters` dict in `UpdateBotRequest`)

---

## Appendix: All files referencing StrategyType / StrategyDefinition / IStrategyFactory

Confirmed by grep across the solution:

```
Core/Domain/BotConfiguration.cs           — defines StrategyDefinition, StrategyType used as field type
Core/Domain/StrategyType.cs               — defines StrategyType enum
Trading/Strategies/IStrategyFactory.cs    — defines IStrategyFactory
Trading/Strategies/StrategyFactory.cs     — implements IStrategyFactory, switch on StrategyType
Infrastructure/Trading/BotRunner.cs       — field _strategyFactory : IStrategyFactory; uses _bot.Strategy
Infrastructure/Persistence/Entities/BotConfigurationDocument.cs  — StrategyDefinitionDocument.Type : StrategyType
Infrastructure/Mapping/Profiles/BotProfile.cs  — CreateMap<StrategyDefinition, StrategyDefinitionDocument>
Infrastructure/Services/BotService.cs     — references StrategyDefinitionDocument in CreateBot()
Api/DTO/BotRequests.cs                    — StrategyDefinitionDto with StrategyType field
Api/DTO/BotResponses.cs                   — BotDto.Strategy : StrategyDefinitionDto
Api/Mapping/BotDtoProfile.cs              — CreateMap<StrategyDefinition, StrategyDefinitionDto>
Api/Extensions/ServiceCollectionExtensions.cs  — services.AddScoped<IStrategyFactory, StrategyFactory>()
Infrastructure.Tests/TradingTests/BotRunnerTests.cs  — Mock<IStrategyFactory>, Mock<ITradingStrategy>
```
