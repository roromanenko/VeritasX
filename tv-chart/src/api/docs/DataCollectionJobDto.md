# DataCollectionJobDto


## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**id** | **string** |  | [optional] [default to undefined]
**symbol** | **string** |  | [optional] [default to undefined]
**fromUtc** | **string** |  | [optional] [default to undefined]
**toUtc** | **string** |  | [optional] [default to undefined]
**interval** | **string** |  | [optional] [default to undefined]
**state** | [**CollectionState**](CollectionState.md) |  | [optional] [default to undefined]
**totalChunks** | **number** |  | [optional] [default to undefined]
**completedChunks** | **number** |  | [optional] [default to undefined]
**createdAt** | **string** |  | [optional] [default to undefined]
**startedAt** | **string** |  | [optional] [default to undefined]
**completedAt** | **string** |  | [optional] [default to undefined]
**errorMessage** | **string** |  | [optional] [default to undefined]

## Example

```typescript
import { DataCollectionJobDto } from './api';

const instance: DataCollectionJobDto = {
    id,
    symbol,
    fromUtc,
    toUtc,
    interval,
    state,
    totalChunks,
    completedChunks,
    createdAt,
    startedAt,
    completedAt,
    errorMessage,
};
```

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)
