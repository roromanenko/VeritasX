# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

VeritasX is an algorithmic cryptocurrency trading platform. It consists of a .NET 9 backend API and a React/TypeScript frontend for monitoring and configuring trading bots connected to Binance.

## Commands

### Backend (.NET)

```bash
# Build solution
dotnet build VeritasX.sln

# Run the API (from repo root or Api/ directory)
dotnet run --project Api

# Run all tests
dotnet test VeritasX.sln

# Run a specific test project
dotnet test Core.Tests
dotnet test Infrastructure.Tests
```

The API runs on `http://localhost:5026` (HTTP) and `https://localhost:7026` (HTTPS). Swagger UI is available at `/swagger` in development.

### Frontend (tv-chart/)

```bash
cd tv-chart

npm install
npm run dev          # Dev server at http://localhost:5173
npm run build        # Production build to dist/
npm run lint         # ESLint
npm run preview      # Preview production build

# Regenerate TypeScript API client from live Swagger (requires API running)
npm run generate-api
```

### Environment Setup

The API requires a `.env` file in `Api/` with:
```
MASTER_ENCRYPTION_KEY=<base64-encoded AES key>
```
See `Api/.env.example` for reference.

## Architecture

### Backend — Layered .NET Solution

```
Core/          → Domain models + interfaces (no framework dependencies)
Infrastructure/ → Service implementations, MongoDB repos, Binance integration, background jobs
Trading/       → Strategy abstractions and implementations (see Trading Layer below)
Application/   → AutoMapper profiles
Api/           → ASP.NET Core controllers, DTOs, DI wiring, SignalR hubs
```

**Dependency flow:** `Api` → `Infrastructure` → `Core`. The `Trading` layer is consumed by infrastructure jobs.

**Dependency injection** is centralized in `Api/Extensions/ServiceCollectionExtensions.cs`. All service registrations happen there, organized into logical groups (caching, HTTP, business, MongoDB, JWT).

### Key Backend Systems

- **MongoDB** — primary data store; repositories in `Infrastructure/Persistence/`
- **Binance.Net** — exchange connectivity; `BinanceService` wraps all exchange calls
- **Background Jobs** (hosted services):
  - `DataCollectorBackgroundService` — collects historical candlestick data
  - `BotRunner` — self-stops after `MaxConsecutiveErrors` (5) consecutive tick failures, setting status to `Error`; the manager cleans up the completed task on the next poll cycle
  - `BotManagerBackgroundService` — manages bot lifecycle (start/stop/execute strategies); cleans up runners whose tasks complete on each 5s poll cycle
  - `DatabaseCleanupJob` — removes stale records per retention config
- **SignalR Hubs** — real-time progress updates at `/jobProgressHub` (data collection) and `/botProgressHub` (bot execution)
- **Encryption** — AES-256-GCM via `EncryptionService`; used to store exchange API keys
- **JWT Auth** — 24h tokens; `[Authorize]` on all protected endpoints

### Trading Layer

- Currently contains strategy abstractions (`ITradingStrategy`, `IStrategyFactory`) and `RebalanceToTargetStrategy`
- **Upcoming major feature: Strategy DSL Editor** — first a text-based internal pseudo-language (DSL), then a visual editor built on Blockly, allowing users to define strategies without writing C#
- When working in `Trading/`, design all abstractions with DSL-driven strategy execution in mind — strategies should be describable as data/configuration, not just code

### Frontend Architecture

- **React Router v7** — routes defined in `App.tsx`
- **API client** — auto-generated TypeScript-Axios client in `src/api/` (generated from Swagger); do not edit manually
- **`apiProvider.ts`** — Axios base config pointing to `http://localhost:5026`
- **SignalR** — connected in `App.tsx`, passed down to pages via props for real-time bot/job status
- **Charts** — `lightweight-charts` for candlestick charts, `recharts` for other data viz

### Key Pages

| Page | Purpose |
|---|---|
| `BotMonitor.tsx` | List all bots, start/stop controls |
| `BotDetail.tsx` | Configure, edit, and monitor a single bot |
| `Chart.tsx` | Candlestick chart with date range selection |
| `Requests.tsx` | Track data collection and backtest jobs |
| `Settings.tsx` | User and exchange connection settings |

### Domain Models (Core/Domain/)

Central entities: `BotConfiguration`, `Candle`, `Trade`, `Order`, `Portfolio`, `User`, `ExchangeConnection`, `DataCollectionJob`. These are plain C# classes with no framework dependencies.

## Code Style

- Follow `.editorconfig` — tabs for indentation, CRLF line endings, PascalCase constants
- Private fields: `_camelCase`
- All I/O operations must use `async`/`await`
- Nullable reference types are enabled — respect all nullability annotations; do not use `!` to suppress warnings without justification

## Testing Rules

- **Never use real API keys or real exchange credentials in tests**
- **Never trigger real orders** — all exchange interactions must be mocked
- All Binance calls in tests must go through a mocked `IBinanceService`
- Mirror the style and structure of existing tests in `Core.Tests` and `Infrastructure.Tests`

## Agent Boundaries

These areas require special care:

- **`EncryptionService` and JWT logic** — sensitive security code; do not modify without explicit user instruction
- **`BotManagerBackgroundService`** — controls live bot execution; changes here can affect running bots in production; proceed with extra caution
- **`tv-chart/src/api/`** — auto-generated; never edit manually, run `npm run generate-api` instead

## Development Notes

- The TypeScript API client (`tv-chart/src/api/`) is generated — run `npm run generate-api` after backend changes that affect the API surface
- CORS in the API allows only `http://localhost:5173` (the Vite dev server)
- `appsettings.Development.json` overrides connection strings for local development
- Test projects mirror the structure of what they test (`Core.Tests` ↔ `Core`, `Infrastructure.Tests` ↔ `Infrastructure`)
