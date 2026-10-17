import { convertToParamMap } from '@angular/router';
import { CATALOG_PAGE_SIZE, parseCatalogQuery, toCatalogParams } from './catalog-query';

describe('catalog query', () => {
  it('reads filters from the URL', () => {
    const query = parseCatalogQuery(
      convertToParamMap({ search: 'linen', brandIds: ['1', '3'], categoryIds: '2', minPrice: '100', inStockOnly: 'true', sort: 'price-asc', page: '2' }),
    );

    expect(query).toEqual({
      search: 'linen',
      brandIds: [1, 3],
      categoryIds: [2],
      minPrice: 100,
      maxPrice: undefined,
      inStockOnly: true,
      sort: 'price-asc',
      page: 2,
      pageSize: CATALOG_PAGE_SIZE,
    });
  });

  it('ignores invalid values', () => {
    const query = parseCatalogQuery(convertToParamMap({ brandIds: ['abc', '-1'], sort: 'hack', page: '0' }));

    expect(query.brandIds).toEqual([]);
    expect(query.sort).toBe('newest');
    expect(query.page).toBe(1);
  });

  it('leaves defaults out of the URL', () => {
    expect(toCatalogParams({ sort: 'newest', page: 1, brandIds: [], inStockOnly: false })).toEqual({
      search: null,
      brandIds: null,
      categoryIds: null,
      minPrice: null,
      maxPrice: null,
      inStockOnly: null,
      sort: null,
      page: null,
    });
  });
});
