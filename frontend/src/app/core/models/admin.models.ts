export interface TopProduct {
  productId: number;
  productName: string;
  unitsSold: number;
  revenue: number;
}

export interface DashboardStats {
  totalRevenue: number;
  paidOrders: number;
  totalProducts: number;
  lowStockProducts: number;
  ordersByStatus: Record<string, number>;
  topProducts: TopProduct[];
}

export interface AdminUser {
  id: string;
  email: string;
  fullName: string;
  roles: string[];
  isLocked: boolean;
  createdAt: string;
}

export interface LegacyStock {
  sku: string;
  shopStock: number;
  warehouseOnHand: number;
  warehouseCode: string;
  lastCountedAt: string;
  difference: number;
}
