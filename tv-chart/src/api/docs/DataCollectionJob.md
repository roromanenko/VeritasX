# DataCollectionJob


## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**id** | [**ObjectId**](ObjectId.md) |  | [optional] [default to undefined]
**userId** | [**ObjectId**](ObjectId.md) |  | [optional] [default to undefined]
**symbol** | **string** |  | [optional] [default to undefined]
**fromUtc** | **string** |  | [optional] [default to undefined]
**toUtc** | **string** |  | [optional] [default to undefined]
**interval** | [**TimeSpan**](TimeSpan.md) |  | [optional] [default to undefined]
**collectionName** | **string** |  | [optional] [default to undefined]
**state** | [**CollectionState**](CollectionState.md) |  | [optional] [default to undefined]
**totalChunks** | **number** |  | [optional] [default to undefined]
**completedChunks** | **number** |  | [optional] [default to undefined]
**createdAt** | **string** |  | [optional] [default to undefined]
**startedAt** | **string** |  | [optional] [default to undefined]
**completedAt** | **string** |  | [optional] [default to undefined]
**errorMessage** | **string** |  | [optional] [default to undefined]
**chunks** | [**Array&lt;DataChunk&gt;**](DataChunk.md) |  | [optional] [default to undefined]

## Example

```typescript
import { DataCollectionJob } from './api';

const instance: DataCollectionJob = {
    id,
    userId,
    symbol,
    fromUtc,
    toUtc,
    interval,
    collectionName,
    state,
    totalChunks,
    completedChunks,
    createdAt,
    startedAt,
    completedAt,
    errorMessage,
    chunks,
};
```

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)
