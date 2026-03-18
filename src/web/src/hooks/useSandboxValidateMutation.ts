import { useMutation } from '@tanstack/react-query';
import { postSandboxValidate } from '../services/sandboxService';

/**
 * Example mutation hook (REQ-FOUNDATION-014.5). Use for forms or actions that POST to the API.
 */
export function useSandboxValidateMutation() {
  return useMutation({
    mutationFn: (message: string) => postSandboxValidate(message),
  });
}
