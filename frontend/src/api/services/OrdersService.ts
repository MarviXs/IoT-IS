import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';
import { useAuthStore } from '@/stores/auth-store.js';

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

  // Vytvorenie novej objednávky
  async createOrder(body: CreateOrderRequest) {
    return await client.POST('/orders', { body });
  }
  async updateOrder(orderId: string, body: UpdateOrderRequest) {
    return await client.PUT('/orders/{id}', { params: { path: { id: orderId } }, body });
  }

  // Aktualizácia objednávky podľa ID
  // async updateOrder(orderId: string, body: UpdateOrderRequest) {
  //   return await client.PUT('/orders/{id}', { body, params: { path: { id: orderId } } });
  // }

  // Odstránenie objednávky podľa ID

  async deleteOrder(orderId: string) {
    return await client.DELETE('/orders/{orderId}', {
      params: {
        path: { orderId },
      },
    });
  }

  async downloadOrderTemplate(orderId: string) {
    const baseUrl = process.env.VITE_API_URL || 'http://localhost:5097/';
    var request = new Request(`${baseUrl}orders/${orderId}/download`);
    const authStore = useAuthStore();

    if (authStore.accessToken) {
      request.headers.set('Authorization', `Bearer ${authStore.accessToken}`);
    }

    return await fetch(request.clone());
  }
}

export default new OrdersService();
