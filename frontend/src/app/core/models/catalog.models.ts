export interface Brand {
  id: number;
  name: string;
  slug: string;
  description: string;
}

export interface Category {
  id: number;
  name: string;
  slug: string;
}

export interface ProductListItem {
  id: number;
  sku: string;
  name: string;
  price: number;
  stockQuantity: number;
  isActive: boolean;
  inStock: boolean;
  brandName: string;
  categoryName: string;
  imageUrl: string | null;
}

export interface ProductImage {
  id: number;
  url: string;
  altText: string;
}

export interface ProductSpecification {
  name: string;
  value: string;
}

export interface ProductDetails {
  id: number;
  sku: string;
  name: string;
  description: string;
  price: number;
  stockQuantity: number;
  isActive: boolean;
  inStock: boolean;
  brand: Brand;
  category: Category;
  images: ProductImage[];
  specifications: ProductSpecification[];
  relatedProducts: ProductListItem[];
}

/** 'stock' (lowest first) is meant for the admin product list. */
export type ProductSort = 'newest' | 'price-asc' | 'price-desc' | 'name' | 'stock';

export interface ProductQuery {
  search?: string;
  brandIds?: number[];
  categoryIds?: number[];
  minPrice?: number;
  maxPrice?: number;
  inStockOnly?: boolean;
  sort?: ProductSort;
  page?: number;
  pageSize?: number;
}

export interface ProductUpsert {
  sku: string;
  name: string;
  description: string;
  price: number;
  stockQuantity: number;
  isActive: boolean;
  brandId: number;
  categoryId: number;
  images: { url: string; altText: string | null }[];
  specifications: ProductSpecification[];
}
