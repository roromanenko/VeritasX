# TradingApi

All URIs are relative to *http://localhost*

|Method | HTTP request | Description|
|------------- | ------------- | -------------|
|[**apiTradingStartHistoryCheckPost**](#apitradingstarthistorycheckpost) | **POST** /api/Trading/startHistoryCheck | |

# **apiTradingStartHistoryCheckPost**
> TradingResultDto apiTradingStartHistoryCheckPost()


### Example

```typescript
import {
    TradingApi,
    Configuration,
    TradeOnHistoryDataRequest
} from './api';

const configuration = new Configuration();
const apiInstance = new TradingApi(configuration);

let tradeOnHistoryDataRequest: TradeOnHistoryDataRequest; // (optional)

const { status, data } = await apiInstance.apiTradingStartHistoryCheckPost(
    tradeOnHistoryDataRequest
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **tradeOnHistoryDataRequest** | **TradeOnHistoryDataRequest**|  | |


### Return type

**TradingResultDto**

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

