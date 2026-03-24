# GetBotStatisticsResponse


## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**botId** | **string** |  | [optional] [default to undefined]
**symbol** | **string** |  | [optional] [default to undefined]
**currentEquity** | **number** |  | [optional] [default to undefined]
**sharpe** | **number** |  | [optional] [default to undefined]
**sortino** | **number** |  | [optional] [default to undefined]
**calmar** | **number** |  | [optional] [default to undefined]
**maxDrawdownPercent** | **number** |  | [optional] [default to undefined]
**volatility** | **number** |  | [optional] [default to undefined]
**winRate** | **number** |  | [optional] [default to undefined]
**profitFactor** | **number** |  | [optional] [default to undefined]
**tradeCount** | **number** |  | [optional] [default to undefined]
**totalRoundTrips** | **number** |  | [optional] [default to undefined]
**totalRealizedPnl** | **number** |  | [optional] [default to undefined]
**totalFees** | **number** |  | [optional] [default to undefined]
**equityCurve** | [**Array&lt;EquityPointDto&gt;**](EquityPointDto.md) |  | [optional] [default to undefined]

## Example

```typescript
import { GetBotStatisticsResponse } from './api';

const instance: GetBotStatisticsResponse = {
    botId,
    symbol,
    currentEquity,
    sharpe,
    sortino,
    calmar,
    maxDrawdownPercent,
    volatility,
    winRate,
    profitFactor,
    tradeCount,
    totalRoundTrips,
    totalRealizedPnl,
    totalFees,
    equityCurve,
};
```

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)
