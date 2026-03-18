/**
 * Centralized API client for all backend HTTP communication.
 * Use this instead of raw fetch/axios in components. Feature-specific services
 * should use this client or instances created with attachBearerTokenInterceptor.
 */
import axios, { AxiosInstance, InternalAxiosRequestConfig } from 'axios';
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

type TokenGetter = () => Promise<string | null>;

let tokenGetter: TokenGetter = async () => null;

/**
 * Registered from AuthTokenBridge when MSAL is active.
 */
export function registerApiTokenGetter(getter: TokenGetter): void {
  tokenGetter = getter;
}

export function resetApiTokenGetter(): void {
  tokenGetter = async () => null;
}

export function attachBearerTokenInterceptor(instance: AxiosInstance): void {
  instance.interceptors.request.use(
    async (cfg: InternalAxiosRequestConfig) => {
      /* Local dev (no VITE_MSAL_CLIENT_ID): never attach Authorization; no MSAL, no tokens. */
      if (!config.auth.isEnabled) {
        return cfg;
      }
      try {
        const token = await tokenGetter();
        if (token) {
          cfg.headers.Authorization = `Bearer ${token}`;
        }
      } catch {
        /* non-fatal: proceed without token */
      }
      return cfg;
    },
    (err) => Promise.reject(err)
  );
}

function createApiClient(): AxiosInstance {
  const instance = axios.create({
    baseURL: config.api.baseUrl,
    headers: {
      'Content-Type': 'application/json',
    },
  });
  attachBearerTokenInterceptor(instance);
  return instance;
}

export const apiClient = createApiClient();

/**
 * Generic REST service aligned with the existing RestService pattern.
 */
export abstract class RestService<T extends Entity> {
  protected client: AxiosInstance;

  public constructor(baseRoute: string) {
    this.client = axios.create({
      baseURL: `${config.api.baseUrl}${baseRoute}`,
      headers: { 'Content-Type': 'application/json' },
    });
    attachBearerTokenInterceptor(this.client);
  }

  public async getList(queryOptions?: QueryOptions): Promise<T[]> {
    const response = await this.client.request<T[]>({
      method: 'GET',
      data: queryOptions,
    });
    return response.data;
  }

  public async get(id: string): Promise<T> {
    const response = await this.client.request<T>({
      method: 'GET',
      url: id,
    });
    return response.data;
  }

  public async save(entity: T): Promise<T> {
    return entity.id ? await this.put(entity) : await this.post(entity);
  }

  public async delete(id: string): Promise<void> {
    await this.client.request<void>({
      method: 'DELETE',
      url: id,
    });
  }

  private async post(entity: T): Promise<T> {
    const response = await this.client.request<T>({
      method: 'POST',
      data: entity,
    });
    return response.data;
  }

  private async put(entity: T): Promise<T> {
    const response = await this.client.request<T>({
      method: 'PUT',
      url: entity.id,
      data: entity,
    });
    return response.data;
  }

  public async patch(id: string, entity: Partial<T>): Promise<T> {
    const response = await this.client.request<T>({
      method: 'PATCH',
      url: id,
      data: entity,
    });
    return response.data;
  }
}
