# DataCollectionApi

All URIs are relative to *http://localhost*

|Method | HTTP request | Description|
|------------- | ------------- | -------------|
|[**apiDataCollectionDataJobIdGet**](#apidatacollectiondatajobidget) | **GET** /api/DataCollection/data/{jobId} | |
|[**apiDataCollectionJobsActiveGet**](#apidatacollectionjobsactiveget) | **GET** /api/DataCollection/jobs/active | |
|[**apiDataCollectionJobsGet**](#apidatacollectionjobsget) | **GET** /api/DataCollection/jobs | |
|[**apiDataCollectionJobsJobIdDelete**](#apidatacollectionjobsjobiddelete) | **DELETE** /api/DataCollection/jobs/{jobId} | |
|[**apiDataCollectionJobsJobIdGet**](#apidatacollectionjobsjobidget) | **GET** /api/DataCollection/jobs/{jobId} | |
|[**apiDataCollectionQueuePost**](#apidatacollectionqueuepost) | **POST** /api/DataCollection/queue | |

# **apiDataCollectionDataJobIdGet**
> Array<CandleDto> apiDataCollectionDataJobIdGet()


### Example

```typescript
import {
    DataCollectionApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new DataCollectionApi(configuration);

let jobId: string; // (default to undefined)

const { status, data } = await apiInstance.apiDataCollectionDataJobIdGet(
    jobId
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **jobId** | [**string**] |  | defaults to undefined|


### Return type

**Array<CandleDto>**

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiDataCollectionJobsActiveGet**
> Array<DataCollectionJobDto> apiDataCollectionJobsActiveGet()


### Example

```typescript
import {
    DataCollectionApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new DataCollectionApi(configuration);

const { status, data } = await apiInstance.apiDataCollectionJobsActiveGet();
```

### Parameters
This endpoint does not have any parameters.


### Return type

**Array<DataCollectionJobDto>**

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiDataCollectionJobsGet**
> Array<DataCollectionJobDto> apiDataCollectionJobsGet()


### Example

```typescript
import {
    DataCollectionApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new DataCollectionApi(configuration);

const { status, data } = await apiInstance.apiDataCollectionJobsGet();
```

### Parameters
This endpoint does not have any parameters.


### Return type

**Array<DataCollectionJobDto>**

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiDataCollectionJobsJobIdDelete**
> apiDataCollectionJobsJobIdDelete()


### Example

```typescript
import {
    DataCollectionApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new DataCollectionApi(configuration);

let jobId: string; // (default to undefined)

const { status, data } = await apiInstance.apiDataCollectionJobsJobIdDelete(
    jobId
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **jobId** | [**string**] |  | defaults to undefined|


### Return type

void (empty response body)

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiDataCollectionJobsJobIdGet**
> DataCollectionJobDto apiDataCollectionJobsJobIdGet()


### Example

```typescript
import {
    DataCollectionApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new DataCollectionApi(configuration);

let jobId: string; // (default to undefined)

const { status, data } = await apiInstance.apiDataCollectionJobsJobIdGet(
    jobId
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **jobId** | [**string**] |  | defaults to undefined|


### Return type

**DataCollectionJobDto**

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiDataCollectionQueuePost**
> QueueJobResponse apiDataCollectionQueuePost()


### Example

```typescript
import {
    DataCollectionApi,
    Configuration,
    QueueDataCollectionRequest
} from './api';

const configuration = new Configuration();
const apiInstance = new DataCollectionApi(configuration);

let queueDataCollectionRequest: QueueDataCollectionRequest; // (optional)

const { status, data } = await apiInstance.apiDataCollectionQueuePost(
    queueDataCollectionRequest
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **queueDataCollectionRequest** | **QueueDataCollectionRequest**|  | |


### Return type

**QueueJobResponse**

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/*+json
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

