# BotsApi

All URIs are relative to *http://localhost*

|Method | HTTP request | Description|
|------------- | ------------- | -------------|
|[**apiBotsGet**](#apibotsget) | **GET** /api/Bots | |
|[**apiBotsIdDelete**](#apibotsiddelete) | **DELETE** /api/Bots/{id} | |
|[**apiBotsIdGet**](#apibotsidget) | **GET** /api/Bots/{id} | |
|[**apiBotsIdStartPost**](#apibotsidstartpost) | **POST** /api/Bots/{id}/start | |
|[**apiBotsIdStopPost**](#apibotsidstoppost) | **POST** /api/Bots/{id}/stop | |
|[**apiBotsIdTradesGet**](#apibotsidtradesget) | **GET** /api/Bots/{id}/trades | |
|[**apiBotsPost**](#apibotspost) | **POST** /api/Bots | |

# **apiBotsGet**
> BotDtoIEnumerableApiResponse apiBotsGet()


### Example

```typescript
import {
    BotsApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new BotsApi(configuration);

const { status, data } = await apiInstance.apiBotsGet();
```

### Parameters
This endpoint does not have any parameters.


### Return type

**BotDtoIEnumerableApiResponse**

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

# **apiBotsIdDelete**
> StringApiResponse apiBotsIdDelete()


### Example

```typescript
import {
    BotsApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new BotsApi(configuration);

let id: string; // (default to undefined)

const { status, data } = await apiInstance.apiBotsIdDelete(
    id
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **id** | [**string**] |  | defaults to undefined|


### Return type

**StringApiResponse**

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

# **apiBotsIdGet**
> BotDtoApiResponse apiBotsIdGet()


### Example

```typescript
import {
    BotsApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new BotsApi(configuration);

let id: string; // (default to undefined)

const { status, data } = await apiInstance.apiBotsIdGet(
    id
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **id** | [**string**] |  | defaults to undefined|


### Return type

**BotDtoApiResponse**

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

# **apiBotsIdStartPost**
> StringApiResponse apiBotsIdStartPost()


### Example

```typescript
import {
    BotsApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new BotsApi(configuration);

let id: string; // (default to undefined)

const { status, data } = await apiInstance.apiBotsIdStartPost(
    id
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **id** | [**string**] |  | defaults to undefined|


### Return type

**StringApiResponse**

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

# **apiBotsIdStopPost**
> StringApiResponse apiBotsIdStopPost()


### Example

```typescript
import {
    BotsApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new BotsApi(configuration);

let id: string; // (default to undefined)

const { status, data } = await apiInstance.apiBotsIdStopPost(
    id
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **id** | [**string**] |  | defaults to undefined|


### Return type

**StringApiResponse**

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

# **apiBotsIdTradesGet**
> BotTradeRecordDtoIEnumerableApiResponse apiBotsIdTradesGet()


### Example

```typescript
import {
    BotsApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new BotsApi(configuration);

let id: string; // (default to undefined)
let limit: number; // (optional) (default to 100)

const { status, data } = await apiInstance.apiBotsIdTradesGet(
    id,
    limit
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **id** | [**string**] |  | defaults to undefined|
| **limit** | [**number**] |  | (optional) defaults to 100|


### Return type

**BotTradeRecordDtoIEnumerableApiResponse**

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

# **apiBotsPost**
> BotDtoApiResponse apiBotsPost()


### Example

```typescript
import {
    BotsApi,
    Configuration,
    CreateBotRequest
} from './api';

const configuration = new Configuration();
const apiInstance = new BotsApi(configuration);

let createBotRequest: CreateBotRequest; // (optional)

const { status, data } = await apiInstance.apiBotsPost(
    createBotRequest
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **createBotRequest** | **CreateBotRequest**|  | |


### Return type

**BotDtoApiResponse**

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

