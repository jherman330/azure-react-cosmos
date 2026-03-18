import 'axios';

declare module 'axios' {
  export interface AxiosRequestConfig {
    /**
     * When set on POST/PUT/PATCH/DELETE, sends Idempotency-Key header (RFC-style).
     */
    idempotencyKey?: string;
  }
}
