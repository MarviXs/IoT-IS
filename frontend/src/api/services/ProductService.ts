import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

export type ProductsQueryParams = paths['/products']['get']['parameters']['query'];
export type ProductsResponse = paths['/products']['get']['responses']['200']['content']['application/json'];

export type CreateProductParams = paths['/products']['post']['requestBody']['content']['application/json'];
export type CreateProductResponse = paths['/products']['post']['responses']['201']['content']['application/json'];

class ProductService {
  async getProducts(queryParams: ProductsQueryParams) {
    return await client.GET('/products', { params: { query: queryParams } });
  }

  async createProduct(body: CreateProductParams) {
    return await client.POST('/products', { body });
  }
}

export default new ProductService();
