import { apiClient } from './apiClient';

export interface SandboxValidateService {
  postValidate(
    message: string,
    options?: { idempotencyKey?: string }
  ): Promise<{ message: string }>;
}

/** Demo write endpoint for mutation examples (REQ-FOUNDATION-014.5). */
export const sandboxService: SandboxValidateService = {
  async postValidate(message, options) {
    const { data } = await apiClient.post<{ message: string }>(
      '/api/v1/sandbox/validate',
      { message },
      options?.idempotencyKey ? { idempotencyKey: options.idempotencyKey } : undefined
    );
    return data as { message: string };
  },
};

/** @deprecated Use sandboxService.postValidate */
export async function postSandboxValidate(
  message: string,
  options?: { idempotencyKey?: string }
): Promise<{ message: string }> {
  return sandboxService.postValidate(message, options);
}
