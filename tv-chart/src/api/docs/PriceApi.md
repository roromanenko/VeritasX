# PriceApi

All URIs are relative to *http://localhost*

|Method | HTTP request | Description|
|------------- | ------------- | -------------|
|[**apiPriceSymbolGet**](#apipricesymbolget) | **GET** /api/Price/{symbol} | |

# **apiPriceSymbolGet**
> Array<Candle> apiPriceSymbolGet()


### Example

```typescript
import {
    PriceApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new PriceApi(configuration);

let symbol: string; // (default to undefined)
let from: string; // (optional) (default to undefined)
let to: string; // (optional) (default to undefined)
let interval: string; // (optional) (default to undefined)

const { status, data } = await apiInstance.apiPriceSymbolGet(
    symbol,
    from,
    to,
    interval
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **symbol** | [**string**] |  | defaults to undefined|
| **from** | [**string**] |  | (optional) defaults to undefined|
| **to** | [**string**] |  | (optional) defaults to undefined|
| **interval** | [**string**] |  | (optional) defaults to undefined|


### Return type

**Array<Candle>**

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | Success |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

