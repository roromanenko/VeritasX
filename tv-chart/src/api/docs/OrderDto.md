# OrderDto


## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**id** | **string** |  | [default to undefined]
**exchangeOrderId** | **string** |  | [default to undefined]
**symbol** | **string** |  | [default to undefined]
**side** | **string** |  | [default to undefined]
**type** | **string** |  | [default to undefined]
**status** | **string** |  | [default to undefined]
**quantity** | **number** |  | [default to undefined]
**quoteQuantity** | **number** |  | [optional] [default to undefined]
**price** | **number** |  | [optional] [default to undefined]
**filledQuantity** | **number** |  | [default to undefined]
**averageFillPrice** | **number** |  | [optional] [default to undefined]
**createdAt** | **string** |  | [default to undefined]
**updatedAt** | **string** |  | [default to undefined]
**executedAt** | **string** |  | [optional] [default to undefined]

## Example

```typescript
import { OrderDto } from './api';

const instance: OrderDto = {
    id,
    exchangeOrderId,
    symbol,
    side,
    type,
    status,
    quantity,
    quoteQuantity,
    price,
    filledQuantity,
    averageFillPrice,
    createdAt,
    updatedAt,
    executedAt,
};
```

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)
