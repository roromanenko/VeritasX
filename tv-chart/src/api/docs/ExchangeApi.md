# ExchangeApi

All URIs are relative to *http://localhost*

|Method | HTTP request | Description|
|------------- | ------------- | -------------|
|[**apiExchangeExchangeConnectivityGet**](#apiexchangeexchangeconnectivityget) | **GET** /api/exchange/{exchange}/connectivity | |
|[**apiExchangeExchangeOrdersOpenGet**](#apiexchangeexchangeordersopenget) | **GET** /api/exchange/{exchange}/orders/open | |
|[**apiExchangeExchangeOrdersPost**](#apiexchangeexchangeorderspost) | **POST** /api/exchange/{exchange}/orders | |
|[**apiExchangeExchangeOrdersSymbolOrderIdDelete**](#apiexchangeexchangeorderssymbolorderiddelete) | **DELETE** /api/exchange/{exchange}/orders/{symbol}/{orderId} | |
|[**apiExchangeExchangeOrdersSymbolOrderIdGet**](#apiexchangeexchangeorderssymbolorderidget) | **GET** /api/exchange/{exchange}/orders/{symbol}/{orderId} | |
|[**apiExchangeExchangePairsSymbolGet**](#apiexchangeexchangepairssymbolget) | **GET** /api/exchange/{exchange}/pairs/{symbol} | |
|[**apiExchangeExchangePortfolioGet**](#apiexchangeexchangeportfolioget) | **GET** /api/exchange/{exchange}/portfolio | |
|[**apiExchangeExchangePriceSymbolGet**](#apiexchangeexchangepricesymbolget) | **GET** /api/exchange/{exchange}/price/{symbol} | |
|[**apiExchangeExchangeServerTimeGet**](#apiexchangeexchangeservertimeget) | **GET** /api/exchange/{exchange}/server-time | |
|[**apiExchangeExchangeTradesSymbolGet**](#apiexchangeexchangetradessymbolget) | **GET** /api/exchange/{exchange}/trades/{symbol} | |

# **apiExchangeExchangeConnectivityGet**
> ConnectivityResponse apiExchangeExchangeConnectivityGet()


### Example

```typescript
import {
    ExchangeApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new ExchangeApi(configuration);

let exchange: ExchangeName; // (default to undefined)

const { status, data } = await apiInstance.apiExchangeExchangeConnectivityGet(
    exchange
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **exchange** | **ExchangeName** |  | defaults to undefined|


### Return type

**ConnectivityResponse**

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

# **apiExchangeExchangeOrdersOpenGet**
> OrdersResponse apiExchangeExchangeOrdersOpenGet()


### Example

```typescript
import {
    ExchangeApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new ExchangeApi(configuration);

let exchange: ExchangeName; // (default to undefined)
let symbol: string; // (optional) (default to undefined)

const { status, data } = await apiInstance.apiExchangeExchangeOrdersOpenGet(
    exchange,
    symbol
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **exchange** | **ExchangeName** |  | defaults to undefined|
| **symbol** | [**string**] |  | (optional) defaults to undefined|


### Return type

**OrdersResponse**

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | OK |  -  |
|**503** | Service Unavailable |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiExchangeExchangeOrdersPost**
> OrderDto apiExchangeExchangeOrdersPost()


### Example

```typescript
import {
    ExchangeApi,
    Configuration,
    PlaceOrderRequest
} from './api';

const configuration = new Configuration();
const apiInstance = new ExchangeApi(configuration);

let exchange: ExchangeName; // (default to undefined)
let placeOrderRequest: PlaceOrderRequest; // (optional)

const { status, data } = await apiInstance.apiExchangeExchangeOrdersPost(
    exchange,
    placeOrderRequest
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **placeOrderRequest** | **PlaceOrderRequest**|  | |
| **exchange** | **ExchangeName** |  | defaults to undefined|


### Return type

**OrderDto**

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/*+json
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**201** | Created |  -  |
|**400** | Bad Request |  -  |
|**404** | Not Found |  -  |
|**503** | Service Unavailable |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiExchangeExchangeOrdersSymbolOrderIdDelete**
> OrderDto apiExchangeExchangeOrdersSymbolOrderIdDelete()


### Example

```typescript
import {
    ExchangeApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new ExchangeApi(configuration);

let exchange: ExchangeName; // (default to undefined)
let symbol: string; // (default to undefined)
let orderId: number; // (default to undefined)

const { status, data } = await apiInstance.apiExchangeExchangeOrdersSymbolOrderIdDelete(
    exchange,
    symbol,
    orderId
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **exchange** | **ExchangeName** |  | defaults to undefined|
| **symbol** | [**string**] |  | defaults to undefined|
| **orderId** | [**number**] |  | defaults to undefined|


### Return type

**OrderDto**

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | OK |  -  |
|**400** | Bad Request |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiExchangeExchangeOrdersSymbolOrderIdGet**
> OrderDto apiExchangeExchangeOrdersSymbolOrderIdGet()


### Example

```typescript
import {
    ExchangeApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new ExchangeApi(configuration);

let exchange: ExchangeName; // (default to undefined)
let symbol: string; // (default to undefined)
let orderId: number; // (default to undefined)

const { status, data } = await apiInstance.apiExchangeExchangeOrdersSymbolOrderIdGet(
    exchange,
    symbol,
    orderId
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **exchange** | **ExchangeName** |  | defaults to undefined|
| **symbol** | [**string**] |  | defaults to undefined|
| **orderId** | [**number**] |  | defaults to undefined|


### Return type

**OrderDto**

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | OK |  -  |
|**503** | Service Unavailable |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiExchangeExchangePairsSymbolGet**
> TradingPairDto apiExchangeExchangePairsSymbolGet()


### Example

```typescript
import {
    ExchangeApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new ExchangeApi(configuration);

let exchange: ExchangeName; // (default to undefined)
let symbol: string; // (default to undefined)

const { status, data } = await apiInstance.apiExchangeExchangePairsSymbolGet(
    exchange,
    symbol
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **exchange** | **ExchangeName** |  | defaults to undefined|
| **symbol** | [**string**] |  | defaults to undefined|


### Return type

**TradingPairDto**

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | OK |  -  |
|**404** | Not Found |  -  |
|**500** | Internal Server Error |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiExchangeExchangePortfolioGet**
> PortfolioDto apiExchangeExchangePortfolioGet()


### Example

```typescript
import {
    ExchangeApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new ExchangeApi(configuration);

let exchange: ExchangeName; // (default to undefined)

const { status, data } = await apiInstance.apiExchangeExchangePortfolioGet(
    exchange
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **exchange** | **ExchangeName** |  | defaults to undefined|


### Return type

**PortfolioDto**

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | OK |  -  |
|**503** | Service Unavailable |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiExchangeExchangePriceSymbolGet**
> PriceDto apiExchangeExchangePriceSymbolGet()


### Example

```typescript
import {
    ExchangeApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new ExchangeApi(configuration);

let exchange: ExchangeName; // (default to undefined)
let symbol: string; // (default to undefined)

const { status, data } = await apiInstance.apiExchangeExchangePriceSymbolGet(
    exchange,
    symbol
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **exchange** | **ExchangeName** |  | defaults to undefined|
| **symbol** | [**string**] |  | defaults to undefined|


### Return type

**PriceDto**

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | OK |  -  |
|**404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiExchangeExchangeServerTimeGet**
> ServerTimeResponse apiExchangeExchangeServerTimeGet()


### Example

```typescript
import {
    ExchangeApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new ExchangeApi(configuration);

let exchange: ExchangeName; // (default to undefined)

const { status, data } = await apiInstance.apiExchangeExchangeServerTimeGet(
    exchange
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **exchange** | **ExchangeName** |  | defaults to undefined|


### Return type

**ServerTimeResponse**

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

# **apiExchangeExchangeTradesSymbolGet**
> TradesResponse apiExchangeExchangeTradesSymbolGet()


### Example

```typescript
import {
    ExchangeApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new ExchangeApi(configuration);

let exchange: ExchangeName; // (default to undefined)
let symbol: string; // (default to undefined)
let orderId: number; // (optional) (default to undefined)
let startTime: string; // (optional) (default to undefined)
let endTime: string; // (optional) (default to undefined)
let limit: number; // (optional) (default to 100)

const { status, data } = await apiInstance.apiExchangeExchangeTradesSymbolGet(
    exchange,
    symbol,
    orderId,
    startTime,
    endTime,
    limit
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **exchange** | **ExchangeName** |  | defaults to undefined|
| **symbol** | [**string**] |  | defaults to undefined|
| **orderId** | [**number**] |  | (optional) defaults to undefined|
| **startTime** | [**string**] |  | (optional) defaults to undefined|
| **endTime** | [**string**] |  | (optional) defaults to undefined|
| **limit** | [**number**] |  | (optional) defaults to 100|


### Return type

**TradesResponse**

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | OK |  -  |
|**503** | Service Unavailable |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

