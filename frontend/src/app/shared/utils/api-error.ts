import { HttpErrorResponse } from '@angular/common/http';

/** Turns an API error (RFC 7807 problem details or validation errors) into one readable sentence. */
export function apiErrorMessage(error: unknown): string {
  if (!(error instanceof HttpErrorResponse)) return 'Something went wrong. Please try again.';
  if (error.status === 0) return 'Cannot reach the server. Is the API running?';

  const problem = error.error;
  if (problem?.errors) {
    const messages = Object.values(problem.errors as Record<string, string[]>).flat();
    if (messages.length) return messages.join(' ');
  }
  return problem?.detail ?? problem?.title ?? 'Something went wrong. Please try again.';
}
