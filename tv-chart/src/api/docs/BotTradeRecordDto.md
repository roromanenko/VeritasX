# BotTradeRecordDto


## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**id** | **string** |  | [optional] [default to undefined]
**botId** | **string** |  | [optional] [default to undefined]
**symbol** | **string** |  | [optional] [default to undefined]
**side** | [**OrderSide**](OrderSide.md) |  | [optional] [default to undefined]
**price** | **number** |  | [optional] [default to undefined]
**quantity** | **number** |  | [optional] [default to undefined]
**reason** | **string** |  | [optional] [default to undefined]
**executedAt** | **string** |  | [optional] [default to undefined]

## Example

```typescript
import { BotTradeRecordDto } from './api';

const instance: BotTradeRecordDto = {
    id,
    botId,
    symbol,
    side,
    price,
    quantity,
    reason,
    executedAt,
};
```

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)
