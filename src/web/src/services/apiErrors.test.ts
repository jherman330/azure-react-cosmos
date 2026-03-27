import { describe, expect, it } from 'vitest';
import axios, { AxiosError } from 'axios';
import { ApiError, isApiError, mapAxiosError, toApiError } from './apiErrors';

function axErr(partial: Partial<AxiosError>): AxiosError {
  return partial as AxiosError;
}

describe('mapAxiosError', () => {
  it('maps 401 to authentication', () => {
    const e = axErr({
      response: {
        status: 401,
        statusText: 'Unauthorized',
        data: {},
        headers: {},
        config: { headers: new axios.AxiosHeaders() } as any,
      },
      config: { _correlationId: 'req-1' } as unknown as AxiosError['config'],
      isAxiosError: true,
    });
    const a = mapAxiosError(e);
    expect(a.kind).toBe('authentication');
    expect(a.statusCode).toBe(401);
    expect(a.correlationId).toBe('req-1');
  });

  it('maps network error to transient', () => {
    const e = axErr({
      code: 'ERR_NETWORK',
      message: 'Network Error',
      config: {} as AxiosError['config'],
      isAxiosError: true,
    });
    const a = mapAxiosError(e);
    expect(a.kind).toBe('transient');
    expect(a.statusCode).toBe(0);
  });

  it('extracts validation field errors from ProblemDetails', () => {
    const e = axErr({
      response: {
        status: 400,
        statusText: 'Bad Request',
        data: {
          title: 'Validation failed',
          errors: { name: ['Required'] },
        },
        headers: {},
        config: { headers: new axios.AxiosHeaders() } as any,
      },
      config: {} as AxiosError['config'],
      isAxiosError: true,
    });
    const a = mapAxiosError(e);
    expect(a.kind).toBe('validation');
    expect(a.fieldErrors).toEqual({ name: ['Required'] });
  });

  it('uses detail for message when present', () => {
    const e = axErr({
      response: {
        status: 404,
        statusText: 'Not Found',
        data: { detail: 'Item gone' },
        headers: {},
        config: { headers: new axios.AxiosHeaders() } as any,
      },
      config: {} as AxiosError['config'],
      isAxiosError: true,
    });
    expect(mapAxiosError(e).message).toBe('Item gone');
  });
});

describe('toApiError', () => {
  it('passes through ApiError', () => {
    const orig = new ApiError('conflict', 'x', 409);
    expect(toApiError(orig)).toBe(orig);
  });

  it('wraps generic Error', () => {
    const a = toApiError(new Error('oops'));
    expect(isApiError(a)).toBe(true);
    expect(a.message).toBe('oops');
  });
});

describe('isApiError', () => {
  it('returns false for non-ApiError', () => {
    expect(isApiError(new Error('x'))).toBe(false);
  });
});
