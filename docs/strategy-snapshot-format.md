# Strategy Snapshot Format

A **strategy snapshot** is a self-contained JSON document that fully describes how to build and run a trading strategy. It is stored in:

- `BotConfiguration.StrategySnapshot` -- the active snapshot used by a running bot.
- `UserStrategyLibrary.SavedDslSnapshot` -- a user's saved copy from the marketplace.

The snapshot is consumed by an `IDslStrategyInterpreter` implementation, which deserializes it into an `ITradingStrategy` instance.

## Schema

| Field | Type | Required | Description |
|---|---|---|---|
| `strategyType` | string | yes | Identifies the strategy kind. Determines which builder logic is used. |
| `dataRequirement` | string | no | `"Ticker"` (default) or `"Kline"`. Controls what market data the bot subscribes to. |
| `interval` | string | no | A `TimeSpan`-parseable string (e.g. `"00:05:00"`). When set, the bot ticks on a fixed schedule instead of reacting to every market update. |
| `config` | object | yes | Strategy-specific configuration. Structure depends on `strategyType`. |
| `overrides` | object | no | Key-value pairs merged into `config` before building. Used by `ParameterOverrides` on `BotConfiguration` and `UserStrategyLibrary` to let users tweak parameters without editing the snapshot itself. |

## Builtin: Rebalance

**`strategyType`**: `"builtin:rebalance"`

Rebalances a portfolio to maintain a target allocation between a trading asset and the baseline (quote) asset.

### Example

```json
{
  "strategyType": "builtin:rebalance",
  "dataRequirement": "Ticker",
  "config": {
    "asset": "BTC",
    "targetPercentage": 50,
    "rebalanceThreshold": 5,
    "minTradeValue": 10
  }
}
```

### Example with overrides

A user saves the strategy from the marketplace and changes the target percentage:

```json
{
  "strategyType": "builtin:rebalance",
  "dataRequirement": "Ticker",
  "config": {
    "asset": "BTC",
    "targetPercentage": 50,
    "rebalanceThreshold": 5,
    "minTradeValue": 10
  },
  "overrides": {
    "targetPercentage": "60"
  }
}
```

Override values are strings that get merged into the config object before deserialization. In this example the effective `targetPercentage` becomes `60`.

## Future: DSL format

When the Strategy DSL Editor is implemented, a new `strategyType` prefix (e.g. `"dsl:..."`) will be added. The `config` field will contain the compiled DSL representation. This document will be updated with the DSL snapshot format at that time.
