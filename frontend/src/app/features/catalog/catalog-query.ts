import { ParamMap, Params } from '@angular/router';
import { ProductQuery, ProductSort } from '../../core/models/catalog.models';

const SORTS: ProductSort[] = ['newest', 'price-asc', 'price-desc', 'name'];
export const CATALOG_PAGE_SIZE = 12;

/** Reads catalog filters from the URL, so filtered pages can be bookmarked and shared. */
export function parseCatalogQuery(params: ParamMap): ProductQuery {
  const numbers = (key: string) => params.getAll(key).map(Number).filter((n) => Number.isInteger(n) && n > 0);
  const optionalNumber = (key: string) => {
    const value = Number(params.get(key));
    return params.has(key) && !Number.isNaN(value) ? value : undefined;
  };
  const sort = params.get('sort') as ProductSort;

  return {
    search: params.get('search') ?? undefined,
    brandIds: numbers('brandIds'),
    categoryIds: numbers('categoryIds'),
    minPrice: optionalNumber('minPrice'),
    maxPrice: optionalNumber('maxPrice'),
    inStockOnly: params.get('inStockOnly') === 'true',
    sort: SORTS.includes(sort) ? sort : 'newest',
    page: Math.max(1, optionalNumber('page') ?? 1),
    pageSize: CATALOG_PAGE_SIZE,
  };
}

/** The inverse of parseCatalogQuery: leaves defaults out so URLs stay short. */
export function toCatalogParams(query: ProductQuery): Params {
  return {
    search: query.search || null,
    brandIds: query.brandIds?.length ? query.brandIds : null,
    categoryIds: query.categoryIds?.length ? query.categoryIds : null,
    minPrice: query.minPrice ?? null,
    maxPrice: query.maxPrice ?? null,
    inStockOnly: query.inStockOnly || null,
    sort: query.sort && query.sort !== 'newest' ? query.sort : null,
    page: query.page && query.page > 1 ? query.page : null,
  };
}
