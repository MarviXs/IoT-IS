import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

export type ProductsQueryParams = paths['/products']['get']['parameters']['query'];
export type ProductsResponse = paths['/products']['get']['responses']['200']['content']['application/json'];

export type ProductResponse = paths['/products/{id}']['get']['responses']['200']['content']['application/json'];

export type CreateProductParams = paths['/products']['post']['requestBody']['content']['application/json'];
export type CreateProductResponse = paths['/products']['post']['responses']['201']['content']['application/json'];

export type UpdateProductRequest = paths['/products/{id}']['put']['requestBody']['content']['application/json'];

class ProductService {
  async getProducts(queryParams: ProductsQueryParams) {
    return await client.GET('/products', { params: { query: queryParams } });
  }

  async getProduct(productId: string) {
    return await client.GET('/products/{id}', { params: { path: { id: productId } } });
  }

  async createProduct(body: CreateProductParams) {
    return await client.POST('/products', { body });
  }

  async updateProduct(productId: string, body: UpdateProductRequest) {
    return await client.PUT('/products/{id}', { body, params: { path: { id: productId } } });
  }

  async deleteProduct(productId: string) {
    return await client.DELETE('/products/{id}', { params: { path: { id: productId } } });
  }
}

export default new ProductService();
