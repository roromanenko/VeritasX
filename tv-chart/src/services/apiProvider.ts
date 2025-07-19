import { AxiosInstance } from 'axios';
import axios from 'axios';
import { Configuration, UserApi } from '../api';
import { useAuth } from '../auth/AuthContext';
import { useNavigate } from 'react-router-dom';

type ApiProvider = {
  getUserApi: () => UserApi;
};

const ApiProvider = (): ApiProvider => {
    const { token: authToken } = useAuth();
    const navigate = useNavigate();

    const config = new Configuration({
        basePath: import.meta.env.VITE_API_URL,
        baseOptions: {
            headers: {
                'Content-Type': 'application/json',
            },
        },
    });

    function getUserApi(){
        return new UserApi(config, config.basePath, getAxiosInstance(config.basePath!));
    }

    function getAxiosInstance(basePath: string): AxiosInstance {
        const instance = axios.create({
            baseURL: basePath,
        });
        instance.interceptors.request.use((config) => {
            if (authToken) {
                config.headers.Authorization = `Bearer ${authToken}`;
            }
            return config;
        });

        instance.interceptors.response.use(
            (res) => res,
            (err) => {
                if (err.response?.status === 401) {
                    navigate("/login");
                }
                return Promise.reject(err);
            }
        );

        return instance;
    }

    return {
        getUserApi
    }
}

export function useApiProvider(): ApiProvider{
    return ApiProvider();
}