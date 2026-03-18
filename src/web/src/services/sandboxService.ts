import { apiClient } from './apiClient';

/** Demo write endpoint for mutation examples (REQ-FOUNDATION-014.5). */
export async function postSandboxValidate(message: string): Promise<{ message: string }> {
  const { data } = await apiClient.post<{ message: string }>('/api/v1/sandbox/validate', {
    message,
  });
  return data as { message: string };
}
