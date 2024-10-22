import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

export type ProductsQueryParams = paths['/products']['get']['parameters']['query'];
export type ProductsResponse = paths['/products']['get']['responses']['200']['content']['application/json'];

class ProductsService {
  async getProducts(queryParams: ProductsQueryParams) {
    return await client.GET('/products', { params: { query: queryParams } });
  }
}

export default new ProductsService();
