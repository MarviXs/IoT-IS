import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';
import { useAuthStore } from '@/stores/auth-store.js';

// Define query parameters and response types for Orders API
export type OrdersQueryParams = paths['/orders']['get']['parameters']['query'];
export type OrdersResponse = paths['/orders']['get']['responses']['200']['content']['application/json'];
export type CreateOrderRequest = paths['/orders']['post']['requestBody']['content']['application/json'];

class OrdersService {
  // Method to fetch orders using the specified query parameters
  async getOrders(queryParams: OrdersQueryParams) {
    return await client.GET('/orders', { params: { query: queryParams } });
  }
  async createOrder(body: CreateOrderRequest) {
    return await client.POST('/orders', { body });
  }

  async downloadOrder(id: string) {
    const baseUrl = process.env.VITE_API_URL || 'http://localhost:5097/';

    const request = new Request(`${baseUrl}orders/${id}/download`);
    const authStore = useAuthStore();

    if (authStore.accessToken) {
      request.headers.set('Authorization', `Bearer ${authStore.accessToken}`);
    }

    const response = await fetch(request.clone());
    if (!response.ok) {
      throw new Error(`Failed to download order: ${response.statusText}`);
    }
    return response;
  }

  // Method to create a new order
  //async createOrder(request: CreateOrderRequest) {
  // try {
  // API request to create a new order
  //  const response = await client.POST('/orders', request);
  //  return { data: response.data, error: null };
  ///} catch (error) {
  // Return error if something goes wrong
  //  return { data: null, error };
  //}
  //}
}

export default new OrdersService();
