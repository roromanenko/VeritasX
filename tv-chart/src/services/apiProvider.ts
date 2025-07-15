import { Configuration, PriceApi } from '../api';

export function getPriceApi() {
    const config = new Configuration({
        basePath: import.meta.env.VITE_API_URL,
        baseOptions: {
            headers: {
                'Content-Type': 'application/json',
            },
        },
    });
    return new PriceApi(config);
}
