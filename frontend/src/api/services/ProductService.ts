import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

export type ProductsQueryParams = paths['/products']['get']['parameters']['query'];
export type ProductsResponse = paths['/products']['get']['responses']['200']['content']['application/json'];

export type ProductResponse = paths['/products/{id}']['get']['responses']['200']['content']['application/json'];

export type CreateProductParams = paths['/products']['post']['requestBody']['content']['application/json'];
export type CreateProductResponse = paths['/products']['post']['responses']['201']['content']['application/json'];

export type CreateProductsFromListParams =
  paths['/products-by-list']['post']['requestBody']['content']['application/json'];
export type CreateProductsFromListResponse =
  paths['/products-by-list']['post']['responses']['201']['content']['application/json'];

export type ProductRequest =
  paths['/products-by-list']['post']['requestBody']['content']['application/json']['products'][number];

export type UpdateProductRequest = paths['/products/{id}']['put']['requestBody']['content']['application/json'];

class ProductService {
  async getProducts(queryParams: ProductsQueryParams) {
    return await client.GET('/products', { params: { query: queryParams } });
  }

  async getProduct(productId: string) {
    return await client.GET('/products/{id}', { params: { path: { id: productId } } });
  }
  async getProductPassport(productId: string) {
    return await client.GET('/products/passport/{id}', { params: { path: { id: productId } } });
  }
  async getProductEan(productId: string) {
    return await client.GET('/products/ean/{id}', { params: { path: { id: productId } } });
  }

  async createProduct(body: CreateProductParams) {
    return await client.POST('/products', { body });
  }

  async createProductsFromList(body: CreateProductsFromListParams) {
    return await client.POST('/products-by-list', { body });
  }

  async updateProduct(productId: string, body: UpdateProductRequest) {
    return await client.PUT('/products/{id}', { body, params: { path: { id: productId } } });
  }

  async deleteProduct(productId: string) {
    return await client.DELETE('/products/{id}', { params: { path: { id: productId } } });
  }

  async downloadProductSticker(productId: string) {
    return await client.GET('/products/{id}/sticker', { params: { path: { id: productId } }, parseAs: 'stream' });
  }
}

export default new ProductService();
