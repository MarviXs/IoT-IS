import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

export type CategoryQueryParams = paths['/product-categories']['get']['parameters']['query'];
export type CategoryResponse = paths['/product-categories']['get']['responses']['200']['content']['application/json'];

class CategoryService {
  async getCategories(queryParams: CategoryQueryParams) {
    return await client.GET('/product-categories', { params: { query: queryParams } });
  }

  async getCategory(categoryId: string) {
    return await client.GET('/product-categories/{id}', { params: { path: { id: categoryId } } });
  }
}

export default new CategoryService();
