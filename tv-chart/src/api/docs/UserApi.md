# UserApi

All URIs are relative to *http://localhost*

|Method | HTTP request | Description|
|------------- | ------------- | -------------|
|[**apiUserExchangesExchangeDelete**](#apiuserexchangesexchangedelete) | **DELETE** /api/User/exchanges/{exchange} | |
|[**apiUserExchangesExchangeGet**](#apiuserexchangesexchangeget) | **GET** /api/User/exchanges/{exchange} | |
|[**apiUserExchangesExchangePost**](#apiuserexchangesexchangepost) | **POST** /api/User/exchanges/{exchange} | |
|[**apiUserExchangesExchangePut**](#apiuserexchangesexchangeput) | **PUT** /api/User/exchanges/{exchange} | |
|[**apiUserExchangesGet**](#apiuserexchangesget) | **GET** /api/User/exchanges | |
|[**apiUserLoginPost**](#apiuserloginpost) | **POST** /api/User/login | |
|[**apiUserMeGet**](#apiusermeget) | **GET** /api/User/me | |
|[**apiUserPasswordPut**](#apiuserpasswordput) | **PUT** /api/User/password | |
|[**apiUserRegisterPost**](#apiuserregisterpost) | **POST** /api/User/register | |

# **apiUserExchangesExchangeDelete**
> StringApiResponse apiUserExchangesExchangeDelete()


### Example

```typescript
import {
    UserApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new UserApi(configuration);

let exchange: ExchangeName; // (default to undefined)

const { status, data } = await apiInstance.apiUserExchangesExchangeDelete(
    exchange
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **exchange** | **ExchangeName** |  | defaults to undefined|


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

# **apiUserExchangesExchangeGet**
> ExchangeConnectionResponseApiResponse apiUserExchangesExchangeGet()


### Example

```typescript
import {
    UserApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new UserApi(configuration);

let exchange: ExchangeName; // (default to undefined)

const { status, data } = await apiInstance.apiUserExchangesExchangeGet(
    exchange
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **exchange** | **ExchangeName** |  | defaults to undefined|


### Return type

**ExchangeConnectionResponseApiResponse**

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

# **apiUserExchangesExchangePost**
> ExchangeConnectionResponseApiResponse apiUserExchangesExchangePost()


### Example

```typescript
import {
    UserApi,
    Configuration,
    AddExchangeConnectionRequest
} from './api';

const configuration = new Configuration();
const apiInstance = new UserApi(configuration);

let exchange: ExchangeName; // (default to undefined)
let addExchangeConnectionRequest: AddExchangeConnectionRequest; // (optional)

const { status, data } = await apiInstance.apiUserExchangesExchangePost(
    exchange,
    addExchangeConnectionRequest
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **addExchangeConnectionRequest** | **AddExchangeConnectionRequest**|  | |
| **exchange** | **ExchangeName** |  | defaults to undefined|


### Return type

**ExchangeConnectionResponseApiResponse**

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

# **apiUserExchangesExchangePut**
> ExchangeConnectionResponseApiResponse apiUserExchangesExchangePut()


### Example

```typescript
import {
    UserApi,
    Configuration,
    UpdateExchangeConnectionRequest
} from './api';

const configuration = new Configuration();
const apiInstance = new UserApi(configuration);

let exchange: ExchangeName; // (default to undefined)
let updateExchangeConnectionRequest: UpdateExchangeConnectionRequest; // (optional)

const { status, data } = await apiInstance.apiUserExchangesExchangePut(
    exchange,
    updateExchangeConnectionRequest
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **updateExchangeConnectionRequest** | **UpdateExchangeConnectionRequest**|  | |
| **exchange** | **ExchangeName** |  | defaults to undefined|


### Return type

**ExchangeConnectionResponseApiResponse**

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

# **apiUserExchangesGet**
> ExchangeNameExchangeConnectionResponseDictionaryApiResponse apiUserExchangesGet()


### Example

```typescript
import {
    UserApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new UserApi(configuration);

const { status, data } = await apiInstance.apiUserExchangesGet();
```

### Parameters
This endpoint does not have any parameters.


### Return type

**ExchangeNameExchangeConnectionResponseDictionaryApiResponse**

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

# **apiUserLoginPost**
> LoginResponseApiResponse apiUserLoginPost()


### Example

```typescript
import {
    UserApi,
    Configuration,
    LoginRequest
} from './api';

const configuration = new Configuration();
const apiInstance = new UserApi(configuration);

let loginRequest: LoginRequest; // (optional)

const { status, data } = await apiInstance.apiUserLoginPost(
    loginRequest
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **loginRequest** | **LoginRequest**|  | |


### Return type

**LoginResponseApiResponse**

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

# **apiUserMeGet**
> UserDtoApiResponse apiUserMeGet()


### Example

```typescript
import {
    UserApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new UserApi(configuration);

const { status, data } = await apiInstance.apiUserMeGet();
```

### Parameters
This endpoint does not have any parameters.


### Return type

**UserDtoApiResponse**

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

# **apiUserPasswordPut**
> StringApiResponse apiUserPasswordPut()


### Example

```typescript
import {
    UserApi,
    Configuration,
    ChangePasswordRequest
} from './api';

const configuration = new Configuration();
const apiInstance = new UserApi(configuration);

let changePasswordRequest: ChangePasswordRequest; // (optional)

const { status, data } = await apiInstance.apiUserPasswordPut(
    changePasswordRequest
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **changePasswordRequest** | **ChangePasswordRequest**|  | |


### Return type

**StringApiResponse**

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

# **apiUserRegisterPost**
> UserDtoApiResponse apiUserRegisterPost()


### Example

```typescript
import {
    UserApi,
    Configuration,
    RegisterRequest
} from './api';

const configuration = new Configuration();
const apiInstance = new UserApi(configuration);

let registerRequest: RegisterRequest; // (optional)

const { status, data } = await apiInstance.apiUserRegisterPost(
    registerRequest
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **registerRequest** | **RegisterRequest**|  | |


### Return type

**UserDtoApiResponse**

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

