import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

// Definovanie typov pre Orders API
export type OrdersQueryParams = paths['/orders']['get']['parameters']['query'];
export type OrdersResponse = paths['/orders']['get']['responses']['200']['content']['application/json'];
export type OrderResponse = paths['/orders/{id}']['get']['responses']['200']['content']['application/json'];
export type CreateOrderRequest = paths['/orders']['post']['requestBody']['content']['application/json'];
export type CreateOrderResponse = paths['/orders']['post']['responses']['201']['content']['application/json'];
export type UpdateOrderRequest = paths['/orders/{id}']['put']['requestBody']['content']['application/json'];

class OrdersService {
  // Načítanie zoznamu objednávok s použitím parametrov dotazu
  async getOrders(queryParams: OrdersQueryParams) {
    return await client.GET('/orders', { params: { query: queryParams } });
  }

  // Načítanie jednej objednávky podľa ID
  async getOrder(orderId: string) {
    return await client.GET('/orders/{id}', { params: { path: { id: orderId } } });
  }
  async getOrderProducts(orderId: string) {
    return await client.GET('/orders/{id}/products', { params: { path: { id: orderId } } });
  }

  // Vytvorenie novej objednávky
  async createOrder(body: CreateOrderRequest) {
    return await client.POST('/orders', { body });
  }
  async updateOrder(orderId: string, body: UpdateOrderRequest) {
    return await client.PUT('/orders/{id}', { params: { path: { id: orderId } }, body });
  }

  async deleteOrder(orderId: string) {
    return await client.DELETE('/orders/{orderId}', {
      params: {
        path: { orderId },
      },
    });
  }

  // Nová metóda pre získanie súhrnu objednávky
  async getSummary(orderId: string) {
    return await client.GET('/orders/{orderId}/summary', { params: { path: { orderId } } });
  }

  async downloadOrderTemplate(orderId: string) {
    return await client.GET('/orders/{id}/download', { params: { path: { id: orderId } }, parseAs: 'stream' });
  }

  async downloadPlantsPassports(orderId: string) {
    return await client.GET('/orders/{id}/passports', { params: { path: { id: orderId } }, parseAs: 'stream' });
  }
}

export default new OrdersService();
