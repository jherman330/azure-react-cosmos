/**
 * Centralized API client for all backend HTTP communication.
 * Use this instead of raw fetch/axios in components. Feature-specific services
 * (e.g. productService) should use this client.
 */
import axios, { AxiosInstance } from 'axios';
import config from '../config';

export interface QueryOptions {
    top?: number;
    skip?: number;
}

export interface Entity {
    id?: string;
    created?: Date;
    updated?: Date;
}

/**
 * Base HTTP client with standard headers and base URL from config.
 * Extend or use for typed API calls. Token attachment can be added here when MSAL is integrated.
 */
function createApiClient(): AxiosInstance {
    return axios.create({
        baseURL: config.api.baseUrl,
        headers: {
            'Content-Type': 'application/json',
        },
    });
}

export const apiClient = createApiClient();

/**
 * Generic REST service aligned with the existing RestService pattern.
 * Use for entity-based resources; wrap in feature-specific services as needed.
 */
export abstract class RestService<T extends Entity> {
    protected client: AxiosInstance;

    public constructor(baseRoute: string) {
        this.client = axios.create({
            baseURL: `${config.api.baseUrl}${baseRoute}`,
            headers: { 'Content-Type': 'application/json' },
        });
    }

    public async getList(queryOptions?: QueryOptions): Promise<T[]> {
        const response = await this.client.request<T[]>({
            method: 'GET',
            data: queryOptions
        });
        return response.data;
    }

    public async get(id: string): Promise<T> {
        const response = await this.client.request<T>({
            method: 'GET',
            url: id
        });
        return response.data;
    }

    public async save(entity: T): Promise<T> {
        return entity.id ? await this.put(entity) : await this.post(entity);
    }

    public async delete(id: string): Promise<void> {
        await this.client.request<void>({
            method: 'DELETE',
            url: id
        });
    }

    private async post(entity: T): Promise<T> {
        const response = await this.client.request<T>({
            method: 'POST',
            data: entity
        });
        return response.data;
    }

    private async put(entity: T): Promise<T> {
        const response = await this.client.request<T>({
            method: 'PUT',
            url: entity.id,
            data: entity
        });
        return response.data;
    }

    public async patch(id: string, entity: Partial<T>): Promise<T> {
        const response = await this.client.request<T>({
            method: 'PATCH',
            url: id,
            data: entity
        });
        return response.data;
    }
}
