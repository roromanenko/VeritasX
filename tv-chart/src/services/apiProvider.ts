import { AxiosInstance } from 'axios';
import axios from 'axios';
import { Configuration, DataCollectionApi, TradingApi, UserApi } from '../api';
import { useAuth } from '../auth/AuthContext';
import { useNavigate } from 'react-router-dom';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

type ApiProvider = {
  getUserApi: () => UserApi;
  getDataCollectionApi: () => DataCollectionApi;
  getTradingApi: () => TradingApi;
  getJobProgressHub: () => HubConnection;
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

    function getDataCollectionApi(){
        return new DataCollectionApi(config, config.basePath, getAxiosInstance(config.basePath!));
    }

    function getTradingApi(){
        return new TradingApi(config, config.basePath, getAxiosInstance(config.basePath!));
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

    function getJobProgressHub(): HubConnection {
        const connection = new HubConnectionBuilder()
            .withUrl(`${config.basePath}/jobProgressHub`, { accessTokenFactory: () => authToken || '' })
            .configureLogging(LogLevel.Information)
            .withAutomaticReconnect()
            .build();

        return connection;
    }

    return {
        getUserApi,
        getDataCollectionApi,
        getTradingApi,
        getJobProgressHub
    }
}

export function useApiProvider(): ApiProvider{
    return ApiProvider();
}