# PlaceOrderRequest


## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**symbol** | **string** |  | [default to undefined]
**side** | [**OrderSide**](OrderSide.md) |  | [default to undefined]
**type** | [**OrderType**](OrderType.md) |  | [default to undefined]
**quantity** | **number** |  | [default to undefined]
**price** | **number** |  | [optional] [default to undefined]

## Example

```typescript
import { PlaceOrderRequest } from './api';

const instance: PlaceOrderRequest = {
    symbol,
    side,
    type,
    quantity,
    price,
};
```

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)
