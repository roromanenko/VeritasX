# BotDto


## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**id** | **string** |  | [optional] [default to undefined]
**name** | **string** |  | [optional] [default to undefined]
**exchange** | [**ExchangeName**](ExchangeName.md) |  | [optional] [default to undefined]
**symbol** | **string** |  | [optional] [default to undefined]
**baseAsset** | **string** |  | [optional] [default to undefined]
**quoteAsset** | **string** |  | [optional] [default to undefined]
**status** | [**BotStatus**](BotStatus.md) |  | [optional] [default to undefined]
**strategy** | [**StrategyDefinitionDto**](StrategyDefinitionDto.md) |  | [optional] [default to undefined]
**riskParameters** | [**RiskParametersDto**](RiskParametersDto.md) |  | [optional] [default to undefined]
**createdAt** | **string** |  | [optional] [default to undefined]
**startedAt** | **string** |  | [optional] [default to undefined]
**stoppedAt** | **string** |  | [optional] [default to undefined]
**errorMessage** | **string** |  | [optional] [default to undefined]

## Example

```typescript
import { BotDto } from './api';

const instance: BotDto = {
    id,
    name,
    exchange,
    symbol,
    baseAsset,
    quoteAsset,
    status,
    strategy,
    riskParameters,
    createdAt,
    startedAt,
    stoppedAt,
    errorMessage,
};
```

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)
