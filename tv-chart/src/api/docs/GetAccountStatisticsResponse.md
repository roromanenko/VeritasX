# GetAccountStatisticsResponse


## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**totalEquityUsd** | **number** |  | [optional] [default to undefined]
**totalRealizedPnl** | **number** |  | [optional] [default to undefined]
**byExchange** | [**Array&lt;ExchangeSummaryDto&gt;**](ExchangeSummaryDto.md) |  | [optional] [default to undefined]
**bots** | [**Array&lt;BotStatisticsSummaryDto&gt;**](BotStatisticsSummaryDto.md) |  | [optional] [default to undefined]

## Example

```typescript
import { GetAccountStatisticsResponse } from './api';

const instance: GetAccountStatisticsResponse = {
    totalEquityUsd,
    totalRealizedPnl,
    byExchange,
    bots,
};
```

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)
