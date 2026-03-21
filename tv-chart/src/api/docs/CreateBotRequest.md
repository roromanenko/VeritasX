# CreateBotRequest


## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**name** | **string** |  | [optional] [default to undefined]
**exchange** | [**ExchangeName**](ExchangeName.md) |  | [optional] [default to undefined]
**symbol** | **string** |  | [optional] [default to undefined]
**baseAsset** | **string** |  | [optional] [default to undefined]
**quoteAsset** | **string** |  | [optional] [default to undefined]
**strategy** | [**StrategyDefinitionDto**](StrategyDefinitionDto.md) |  | [optional] [default to undefined]
**riskParameters** | [**RiskParametersDto**](RiskParametersDto.md) |  | [optional] [default to undefined]

## Example

```typescript
import { CreateBotRequest } from './api';

const instance: CreateBotRequest = {
    name,
    exchange,
    symbol,
    baseAsset,
    quoteAsset,
    strategy,
    riskParameters,
};
```

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)
