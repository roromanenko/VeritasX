# UserApi

All URIs are relative to *http://localhost*

|Method | HTTP request | Description|
|------------- | ------------- | -------------|
|[**apiUserLoginPost**](#apiuserloginpost) | **POST** /api/User/login | |
|[**apiUserMeGet**](#apiusermeget) | **GET** /api/User/me | |
|[**apiUserPasswordPut**](#apiuserpasswordput) | **PUT** /api/User/password | |
|[**apiUserRegisterPost**](#apiuserregisterpost) | **POST** /api/User/register | |

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
|**200** | Success |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiUserMeGet**
> UserResponseApiResponse apiUserMeGet()


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

**UserResponseApiResponse**

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
|**200** | Success |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiUserRegisterPost**
> UserResponseApiResponse apiUserRegisterPost()


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

**UserResponseApiResponse**

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/*+json
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | Success |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

