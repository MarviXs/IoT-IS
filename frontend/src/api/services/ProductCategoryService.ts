import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

export type ProductCategoryParams = paths['/product-categories']['get']['parameters']['query'];
export type ProductCategoryResponse =
  paths['/product-categories']['get']['responses']['200']['content']['application/json'];

class ProductCategoryService {
  async getProductCategories(queryParams: ProductCategoryParams) {
    return await client.GET('/product-categories', { params: { query: queryParams } });
  }
}

export default new ProductCategoryService();
