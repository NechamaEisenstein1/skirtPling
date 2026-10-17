import { HttpErrorResponse } from '@angular/common/http';
import { apiErrorMessage } from './api-error';

describe('apiErrorMessage', () => {
  it('uses the problem details "detail"', () => {
    const error = new HttpErrorResponse({ status: 409, error: { title: 'Conflict', detail: 'Only 2 left in stock.' } });
    expect(apiErrorMessage(error)).toBe('Only 2 left in stock.');
  });

  it('joins validation errors', () => {
    const error = new HttpErrorResponse({ status: 400, error: { errors: { Email: ['Email is required.'], Password: ['Too short.'] } } });
    expect(apiErrorMessage(error)).toBe('Email is required. Too short.');
  });

  it('explains when the API is unreachable', () => {
    expect(apiErrorMessage(new HttpErrorResponse({ status: 0 }))).toContain('Cannot reach the server');
  });
});
