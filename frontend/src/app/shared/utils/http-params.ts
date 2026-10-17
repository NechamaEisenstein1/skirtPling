import { HttpParams } from '@angular/common/http';

/** Converts a plain query object into HttpParams, skipping empty values and repeating keys for arrays. */
export function toHttpParams(query: object): HttpParams {
  let params = new HttpParams();
  for (const [key, value] of Object.entries(query)) {
    if (value === undefined || value === null || value === '') continue;
    if (Array.isArray(value)) {
      value.forEach((item) => (params = params.append(key, String(item))));
    } else {
      params = params.set(key, String(value));
    }
  }
  return params;
}
