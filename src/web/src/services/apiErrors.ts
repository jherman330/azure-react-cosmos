import axios, { AxiosError } from 'axios';

/** Normalized API failure categories for UI and logging (REQ-FOUNDATION-016). */
export type ApiErrorKind =
  | 'validation'
  | 'authentication'
  | 'authorization'
  | 'not_found'
  | 'conflict'
  | 'transient'
  | 'unknown';

const PROBLEM_TYPES: Record<string, ApiErrorKind> = {
  'https://tools.ietf.org/html/rfc7231#section-6.5.1': 'validation',
  'https://tools.ietf.org/html/rfc7235#section-3.1': 'authentication',
  'https://tools.ietf.org/html/rfc7231#section-6.5.3': 'authorization',
  'https://tools.ietf.org/html/rfc7231#section-6.5.4': 'not_found',
  'https://tools.ietf.org/html/rfc7231#section-6.5.8': 'conflict',
};

export interface ProblemDetailsLike {
  title?: string;
  detail?: string;
  status?: number;
  type?: string;
  errors?: Record<string, string[]>;
  traceId?: string;
}

export class ApiError extends Error {
  override readonly name = 'ApiError';

  constructor(
    public readonly kind: ApiErrorKind,
    message: string,
    public readonly statusCode: number,
    public readonly correlationId?: string,
    public readonly fieldErrors?: Record<string, string[]>,
    public readonly raw?: unknown
  ) {
    super(message);
    Object.setPrototypeOf(this, ApiError.prototype);
  }
}

export function isApiError(e: unknown): e is ApiError {
  return e instanceof ApiError;
}

function kindFromStatus(status: number): ApiErrorKind {
  if (status === 401) return 'authentication';
  if (status === 403) return 'authorization';
  if (status === 404) return 'not_found';
  if (status === 409) return 'conflict';
  if (status === 408 || status === 429) return 'transient';
  if (status >= 500 && status <= 599) return 'transient';
  if (status === 400 || status === 422) return 'validation';
  return 'unknown';
}

function parseBody(data: unknown): ProblemDetailsLike | null {
  if (data === null || data === undefined) return null;
  if (typeof data !== 'object') return null;
  const o = data as Record<string, unknown>;
  if (
    typeof o.title === 'string' ||
    typeof o.detail === 'string' ||
    typeof o.status === 'number' ||
    o.errors !== undefined
  ) {
    return data as ProblemDetailsLike;
  }
  return null;
}

function collectFieldErrors(body: ProblemDetailsLike | null): Record<string, string[]> | undefined {
  if (!body?.errors || typeof body.errors !== 'object') return undefined;
  const out: Record<string, string[]> = {};
  for (const [k, v] of Object.entries(body.errors)) {
    if (Array.isArray(v) && v.every((x) => typeof x === 'string')) {
      out[k] = v;
    }
  }
  return Object.keys(out).length > 0 ? out : undefined;
}

function messageFrom(body: ProblemDetailsLike | null, fallback: string): string {
  const d = body?.detail?.trim();
  if (d) return d;
  const t = body?.title?.trim();
  if (t) return t;
  return fallback;
}

/**
 * Maps axios failures to {@link ApiError}. Use {@link isApiError} in catch blocks.
 */
export function mapAxiosError(err: AxiosError<unknown>): ApiError {
  const cfg = err.config as
    | (typeof err.config & { _correlationId?: string })
    | undefined;
  const sentCorrelation = cfg?._correlationId;
  const headerCorr =
    err.response?.headers?.['x-correlation-id'] ??
    err.response?.headers?.['X-Correlation-ID'];
  const correlationId =
    (typeof headerCorr === 'string' ? headerCorr : undefined) ?? sentCorrelation;

  if (!err.response) {
    const code = err.code;
    const transient =
      code === 'ERR_NETWORK' ||
      code === 'ECONNABORTED' ||
      code === 'ETIMEDOUT';
    return new ApiError(
      transient ? 'transient' : 'unknown',
      err.message || 'Network error',
      0,
      correlationId,
      undefined,
      undefined
    );
  }

  const status = err.response.status;
  const body = parseBody(err.response.data);
  let kind = kindFromStatus(status);
  if (body?.type && PROBLEM_TYPES[body.type]) {
    kind = PROBLEM_TYPES[body.type]!;
  }
  if (kind === 'unknown' && status === 400 && collectFieldErrors(body)) {
    kind = 'validation';
  }

  const fieldErrors = collectFieldErrors(body);
  const message = messageFrom(
    body,
    err.response.statusText || `Request failed (${status})`
  );

  return new ApiError(
    kind,
    message,
    status,
    correlationId,
    fieldErrors,
    err.response.data
  );
}

export function toApiError(err: unknown): ApiError {
  if (isApiError(err)) return err;
  if (axios.isAxiosError(err)) return mapAxiosError(err);
  if (err instanceof Error) {
    return new ApiError('unknown', err.message, 0, undefined, undefined, undefined);
  }
  return new ApiError('unknown', 'Unexpected error', 0, undefined, undefined, err);
}
